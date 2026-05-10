function applyStyleForElement(data) {
    document.getElementById(data.id).style[data.attrib] = data.value;
}

function applyStyleForElementClass(data) {
    for (let classReference of document.getElementsByClassName(data.className)) {
        classReference.style[data.attrib] = data.value;
    }   
}

function setInnerHTMLForElement(data) {
    document.getElementById(data.id).innerHTML = data.value;
}

function disableElement(data) {
    document.getElementById(data.id).disabled = data.value;
}

function copyToClipboard(text) {
    navigator.clipboard.writeText(text).then(function () {
        //console.log('Text copied to clipboard');
    }).catch(function (err) {
        console.error('Could not copy text: ', err);
    });
}

function DownloadFile(filename, content) {
    const blob = new Blob([content], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

async function UploadFilePrompt() {
    return new Promise((resolve) => {
        const input = document.createElement('input');
        input.type = 'file';
        input.accept = '.mylang,.asm,.bin,.txt,.csv,*';
        input.onchange = async () => {
            if (input.files.length > 0) {
                const text = await input.files[0].text();
                resolve(text);
            } else {
                resolve(null);
            }
        };
        input.click();
    });
}