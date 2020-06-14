using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ICSharpCode.AvalonEdit;
using System.Windows.Media;

namespace AsmIDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Vriables

        #region Serial Port

        SerialPort sp;
        const string startChar = "_";
        const int baudRate = 115200;

        #endregion

        bool serialConnected = false;
        int totalLengthGlobal = 0;
        int memSizeGlobal = 0;
        int memDataposition = 0;
        string[] data;
        string memAdressLength = ""; 
        string memSize = "";
        string memWordLength = "";

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Events

        #region File Buttons Events

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            getFileDialog(new OpenFileDialog(), codeEditor);
        }

        private void saveFileButton_Click(object sender, RoutedEventArgs e)
        {
            getFileDialog(new SaveFileDialog(), codeEditor);
        }

        private void openDefinitionFileButton_Click(object sender, RoutedEventArgs e)
        {
            getFileDialog(new OpenFileDialog(), assemblyDefinitionEditor);
        }

        private void saveDefinitionFileButton_Click(object sender, RoutedEventArgs e)
        {
            getFileDialog(new SaveFileDialog(), assemblyDefinitionEditor);
        }

        #endregion

        private void programButton_Click(object sender, RoutedEventArgs e)
        {
            memAdressLength = memAdressLengthTextBox.Text;
            memWordLength = memWordLengthTextBox.Text;
            memSize = memSizeTextBox.Text;

            string binaryRAMContents = binaryCodeEditor.Text;
            int totalLength = int.Parse(memAdressLength) + int.Parse(memWordLength);
            totalLengthGlobal = totalLength;
            memSizeGlobal = int.Parse(memSize);

            data = new string[int.Parse(memSize)];

            if (codeEditor.LineCount > int.Parse(memSize))
            {
                statusUpdate("Not all memory locations have a value!");
                //Terminate method execution.
                return;
            }

            if (memAdressLength.Length < 2)
            {
                memAdressLength = memAdressLength.PadLeft(2, '0');
            }

            if (memWordLength.Length < 2)
            {
                memWordLength = memWordLength.PadLeft(2, '0');
            }

            if (memSize.Length < 5)
            {
                memSize = memSize.PadLeft(5, '0');
            }

            //Get rid of spaces and new lines.
            binaryRAMContents = Regex.Replace(binaryRAMContents, @"\r\n?|\n", "");
            binaryRAMContents = Regex.Replace(binaryRAMContents, @"\s+", "");

            //Fill array with mem. data.
            for (int i = 0; i < int.Parse(memSize); i++)
            {
                string memAddressContents = "";
                for (int j = 0; j < totalLength; j++)
                {
                    memAddressContents += binaryRAMContents[j + (i * totalLength)];
                }
                data[i] = memAddressContents;
            }

            string commandString = "prog" + memAdressLength + memWordLength + memSize/* + binaryRAMContents*/;

            sendSerialPackets(commandString);
        }

        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            sendSerial("rset");
        }

        private void clockStartButton_Click(object sender, RoutedEventArgs e)
        {
            sendSerial("strt");
        }

        private void clockStopButton_Click(object sender, RoutedEventArgs e)
        {
            sendSerial("stop");
        }

        private void clockStepButton_Click(object sender, RoutedEventArgs e)
        {
            sendSerial("step");
        }

        private void clockSpeedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            sendSerial("clks" + ((ComboBoxItem)clockSpeedComboBox.SelectedItem).Tag.ToString());
        }

        private void comPortSelectionComboBox_DropDownOpened(object sender, EventArgs e)
        {
            //Clear all existing COM ports from the combo box.
            comPortSelectionComboBox.Items.Clear();
            //Get current COM ports. 
            getCOMPorts();
        }

        #endregion

        private void serialConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string port = comPortSelectionComboBox.SelectedValue.ToString();
                

                if (serialConnected)
                {
                    sp.Close();
                    serialConnected = false;      

                    statusUpdate("Serial port closed.");
                    var converter = new BrushConverter();
                    serialConnectButton.Background = (Brush)converter.ConvertFromString("#FFDDDDDD");
                }
                else
                {
                    //
                    sp = new SerialPort(port, baudRate);                
                    sp.NewLine = "\n";         

                    sp.Open();
                    serialConnected = true;                  

                    statusUpdate("Serial port opened.");
                    var converter = new BrushConverter();
                    serialConnectButton.Background = (Brush)converter.ConvertFromString("#a3ff9e");

                    //Clear I/O buffers.
                    sp.DiscardOutBuffer();
                    sp.DiscardInBuffer();

                    //Add event when data is received un the seial buffer.
                    sp.DataReceived += OnScan;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void statusUpdate(string text)
        {
            statusLabel.Content = text;
        }

        void getCOMPorts()
        {
            string[] spNames = SerialPort.GetPortNames();

            foreach (string spName in spNames)
            {
                comPortSelectionComboBox.Items.Add(spName);
            }
        }

        private void codeEditor_TextChanged(object sender, EventArgs e)
        {          
            if (codeEditor.LineCount > int.Parse(memSizeTextBox.Text))
            {
                //Warn user about too many lines.
                statusUpdate("There is too much data(lines) for the curet RAM size!");
            }

            //Check if the assembly is defined.
            if (assemblyDefinitionEditor.Text == "")
            {
                //Warn user about the missing asssembly definition.
                statusUpdate("The assembly is not is not defined!");
                //Terminate method execution.
                return;
            }

            fillBinaryEditor();  
        }

        void fillBinaryEditor()
        {
            if (assemblyDefinitionEditor.LineCount == 0)
            {
                statusUpdate("Define the assembly!");
                //Terminate method execution.
                return;
            }

            string[] lines = getTextLines(codeEditor);
            Dictionary<string, string> instructionsList = getInstructions(assemblyDefinitionEditor);
            List<string> machineCode = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                string[] assembly = lines[i].Split(new char[] { ' ', '\t' });

                string key = assembly[0].ToUpper();

                if (instructionsList.ContainsKey(key.ToUpper()))
                {
                    int value;
                    string opCode;
                    string valueString = "0000";

                    instructionsList.TryGetValue(assembly[0].ToUpper(), out opCode);

                    if (assembly.Length == 2)
                    {
                        if (int.TryParse(assembly[1], out value))
                        {
                            if (value > Math.Pow(2, 4))
                            {
                                //Warn user about the number being too large.
                                statusUpdate("The number is too big!");
                            }
                            else
                            {
                                //Convert bin to dec.
                                string numericValue = Convert.ToString(value, 2);
                                //Reverse LSB and MSB
                                numericValue = stringReverse(numericValue);
                                //Make 8 bit value.
                                valueString = numericValue.PadRight(4, '0');
                            }
                        }
                    }

                    //Add to binary editor.
                    machineCode.Add(opCode + " " + valueString);
                }
                else if (key == "")
                {
                    string filler = "";
                    //Make n bit long filler value.
                    filler = filler.PadRight(8, '0');
                    //Format output.
                    filler = filler.Insert(4, " ");
                    //Add to binary editor.
                    machineCode.Add(filler);
                }
                else
                {
                    int value;

                    if (int.TryParse(lines[i], out value))
                    {
                        if (value >= Math.Pow(2, 8))
                        {
                            //Warn user about the number being too large.
                            statusUpdate("The number is too big!");
                        }
                        else
                        {
                            //Convert dec to bin.
                            string numericValue = Convert.ToString(value, 2);
                            //Reverse LSB and MSB
                            numericValue = stringReverse(numericValue);
                            //Make 8 bit value.
                            numericValue = numericValue.PadRight(8, '0');
                            //Format output.
                            numericValue = numericValue.Insert(4, " ");
                            //Add to binary editor.
                            machineCode.Add(numericValue);
                        }
                    }
                }
            }

            addToBinaryEditor(machineCode);
        }

        string stringReverse(string text)
        {
            if (text == null) return null;

            char[] array = text.ToCharArray();
            Array.Reverse(array);
            return new String(array);
        }

        void addToBinaryEditor(List<string> machineCode)
        {
            machineCode = addMemAddLocation(machineCode);

            binaryCodeEditor.Clear();

            bool isFirst = true;

            foreach (var item in machineCode)
            {
                if (isFirst)
                {
                    isFirst = false;
                    binaryCodeEditor.AppendText(item);
                }
                else
                {
                    binaryCodeEditor.AppendText("\r\n" + item);
                }
            }
        }

        private List<string> addMemAddLocation(List<string> machineCode)
        {
            for (int i = 0; i < machineCode.Count; i++)
            {
                //Convert dec to bin.
                string memAdress = Convert.ToString(i, 2);
                //Reverse LSB and MSB
                memAdress = stringReverse(memAdress);
                //Make 4 bit value.
                memAdress = memAdress.PadRight(4, '0');
                //Format output.
                memAdress = "   " + memAdress;
                //Reassign.
                machineCode[i] = machineCode[i] + memAdress;
            }

            return machineCode;
        }

        void sendSerial(string data)
        {
            if (serialConnected)
                {
                    try
                    {
                        //Send data. 
                        sp.WriteLine(startChar + data);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
             else
             {
                statusUpdate("Open the serial port before sending data!");
             }
        }

        void sendSerialPackets(string commandString)
        {
            if (serialConnected)
            {
                try
                {
                    //Send data.               
                    sp.WriteLine(startChar + "prog" + memAdressLength + memWordLength + memSize + data[memDataposition]);
                    //Advance mem. data array index for the next packet.
                    memDataposition++;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                statusUpdate("Open serial port before sending data!");
            }
        }

        void OnScan(object sender, SerialDataReceivedEventArgs args)
        {
            //Wait for data.
            Thread.Sleep(100);
            
            //Get serial port instance.
            SerialPort port = sender as SerialPort;

            //Read serial buffer and remove null termination.
            string line = port.ReadExisting();
            line = line.Replace("\0", "");

            if (line == "ok")
            {
                if (memDataposition < memSizeGlobal)
                {
                    //Send data.  
                    sp.WriteLine(startChar + "prog" + memAdressLength + memWordLength + memSize + data[memDataposition]);
                    //Advance mem. data array index for the next packet.
                    memDataposition++;
                }
                else
                {
                    //Doesn't get executed.
                    /*
                    //Tell mcu we are done.
                    sp.WriteLine("done");
                    //Reset.
                    memDataposition = 0;
                    //Update status in GUI.
                    //statusUpdate("Programming Done!"); //thread error
                    */
                }
            }
            else if(line == "prog done")
            {
                //Tell mcu we are done.
                sp.WriteLine("done");
                //Reset.
                memDataposition = 0;
                //Update status in GUI.
                //statusUpdate("Programming Done!"); //thread error
            }
        }

        void saveFromEditor(TextEditor editor, string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                string[] lines = getTextLines(editor);

                for (int i = 0; i < lines.Length; i++)
                {
                    writer.WriteLine(lines[i]);
                }
            }
        }
    
        void loadIntoEditor(TextEditor editor, string path)
        {
            string line;
            bool isFirst = true;

            editor.Clear();

            using (StreamReader reader = new StreamReader(path))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        editor.AppendText(line);
                    }
                    else
                    {
                        editor.AppendText("\r\n" + line);
                    }
                }
            }

            fillBinaryEditor();
        }

        static string[] getTextLines(TextEditor codeEditor)
        {
            return codeEditor.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);
        }

        private void getFileDialog(FileDialog fileDialog, TextEditor editor)
        {
            // Set filter for file extension and default file extension 
            fileDialog.DefaultExt = ".txt";
            fileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
            // Set initial directory    
            //fileDialog.InitialDirectory;

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = fileDialog.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filePath = fileDialog.FileName;

                if (fileDialog is SaveFileDialog)
                    saveFromEditor(editor, filePath);
                else
                    loadIntoEditor(editor, filePath);
            }
        }

        static Dictionary<string, string> getInstructions(TextEditor assemblyDefinitionEditor)
        {
            Dictionary<string, string> instructionsList = new Dictionary<string, string>();

            string[] assemblyDefinitions = assemblyDefinitionEditor.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            for (int i = 0; i < assemblyDefinitions.Length; i++)
            {
                string[] asmDefArray = assemblyDefinitions[i].Split('=');

                string instruction = asmDefArray[0];
                string opCode = "";

                if (asmDefArray.Length > 1)
                {
                    opCode = asmDefArray[1];
                }
                
                instructionsList.Add(instruction.ToUpper(), opCode);
            }

            return instructionsList;
        }
    }
}