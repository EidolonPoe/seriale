// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.IO.Ports;

internal class Program
{
    static bool _continue;
    static SerialPort _serialPort;
    static string dataBuffer = "";

    private static async Task<int> Main(string[] args)
    {
        var portName = new Option<string>(
            name: "--port",
            description: "Il nome della porta seriale",
            getDefaultValue: () => "COM3"
            );

        var baudRate = new Option<int>(
            name: "--baud",
            description: "Velocità flusso dati",
            getDefaultValue: () => 9600
            );

        var parity = new Option<Parity>(
            name: "--parity",
            description: "Bit di parità",
            getDefaultValue: () => Parity.None
            );

        var dataBits = new Option<int>(
            name: "--databits",
            description: "Numero di bit",
            getDefaultValue: () => 8
            );

        var stopBits = new Option<StopBits>(
            name: "--stopbits",
            description: "Bit di stop",
            getDefaultValue: () => StopBits.One
            );


        var rootCommand = new RootCommand("Applicazione per comunicare con una porta seriale");


        var writeCommand = new Command("write", "write and display the Port")
        {
            portName,
            baudRate,
            parity,
            dataBits,
            stopBits
        };

        rootCommand.AddCommand(writeCommand);



        writeCommand.SetHandler((port, baud, parityBit, dataBit, stopBit) =>
        {
            WritePort(port, baud, parityBit, dataBit, stopBit);
        }, portName, baudRate, parity, dataBits, stopBits);

        return await rootCommand.InvokeAsync(args);
    }


    private static void WritePort(string port, int baud, Parity parityBit, int dataBit, StopBits stopBit)
    {
        _serialPort = new SerialPort(port, baud, parityBit, dataBit, stopBit);

        StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;

        try
        {
            _serialPort.Open();
            Console.WriteLine($"La porta {port} è  stata aperta");
            _continue = true;
            Console.WriteLine("Scrivi 'quit' per uscire");
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(ReadPort);

            while (_continue)
            {

                var message = Console.ReadLine();

                if (stringComparer.Equals("quit", message))
                {
                    _continue = false;
                }
                else if (!string.IsNullOrEmpty(message))
                {
                    _serialPort.WriteLine(message);
                    Console.WriteLine($"Inviato: {message}");

                }
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Dispose();
                _serialPort.Close();
                Console.WriteLine("La porta seriale è stata chiusa.");
            }
        }
    }


    private static void ReadPort(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            string data = _serialPort.ReadExisting();
            dataBuffer += data;

            if (dataBuffer.EndsWith('\n'))
            {
                Console.WriteLine($"Dati ricevuti: {dataBuffer}");

                dataBuffer = "";
                Console.WriteLine("Scrivi 'quit' per uscire o digita un nuovo messaggio da inviare");
                Console.WriteLine();
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine(ex);
        }
    }
}