let serialPort = null;
let serialReader = null;
let serialWriter = null;
let dotNetSerialRef = null;

function SerialIsSupported() {
    return 'serial' in navigator;
}

async function SerialGetPorts() {
    //Web Serial API doesn't enumerate ports without prior user grant.
    //Returns previously granted ports.
    const ports = await navigator.serial.getPorts();

    return ports.map((p, i) => {
        const info = p.getInfo();
        return `Port ${i} (vendor: ${info.usbVendorId ?? 'N/A'})`;
    });
}

async function SerialConnect(baudRate, dotNetRef) {
    try {
        serialPort = await navigator.serial.requestPort();
        await serialPort.open({ baudRate: baudRate });

        dotNetSerialRef = dotNetRef;
        serialBuffer = "";
        serialTimeout = null;

        const decoder = new TextDecoderStream();
        serialPort.readable.pipeTo(decoder.writable);
        serialReader = decoder.readable.getReader();
        _readLoop();

        return true;
    } catch (ex) {
        console.error("Serial connect error:", ex);
        return false;
    }
}

async function SerialDisconnect() {
    try {
        if (serialTimeout) {
            clearTimeout(serialTimeout);
            serialTimeout = null;
        }

        serialBuffer = "";
        if (serialReader) {
            await serialReader.cancel();
            serialReader = null;
        }

        if (serialPort) {
            await serialPort.close();
            serialPort = null;
        }

        dotNetSerialRef = null;

        return true;
    } catch (ex) {
        console.error("Serial disconnect error:", ex);
        return false;
    }
}

async function SerialSend(data) {
    if (!serialPort || !serialPort.writable) {
        console.error("Serial port not open.");
        return false;
    }

    try {
        const encoder = new TextEncoder();
        const writer = serialPort.writable.getWriter();
        await writer.write(encoder.encode(data));
        writer.releaseLock();

        return true;
    } catch (ex) {
        console.error("Serial send error:", ex);
        return false;
    }
}

let serialBuffer = "";
let serialTimeout = null;

async function _readLoop() {
    try {
        while (true) {
            const { value, done } = await serialReader.read();

            if (done)
                break;

            if (value && dotNetSerialRef) {
                //Remove null characters, matching the AsmIDE behavior.
                serialBuffer += value.replace(/\0/g, '');

                //Clear any existing timeout.
                if (serialTimeout)
                    clearTimeout(serialTimeout);

                //Wait 100ms for more data before processing (matches AsmIDE's Thread.Sleep(100)).
                serialTimeout = setTimeout(async () => {
                    const message = serialBuffer.trim();

                    serialBuffer = "";
                    if (message.length > 0)
                        await dotNetSerialRef.invokeMethodAsync('OnSerialDataReceived', message);

                }, 100);
            }
        }
    } catch (ex) {
        console.error("Serial read error:", ex);
    }
}