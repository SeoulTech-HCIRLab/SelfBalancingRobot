using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

public class Data
{
    public string receive { get; set; }
    public string send { get; set; }
}

public class Serial
{
    Data data = new Data();
    SerialPort stream;
    bool sw = false;

    public Serial(string port, int baudrate, bool sw)
    {
        this.sw = sw;

        if (this.sw)
        {
            stream = new SerialPort(port, baudrate, Parity.None, 8, StopBits.One);
            stream.Open();

            var cancel = new CancellationTokenSource();
            var ReadTask = Task.Factory.StartNew(() =>
            {
                while (!cancel.IsCancellationRequested)
                {
                    data.receive = stream.ReadLine();
                    if (data.receive.ToLower() == "quit")
                    {
                        Environment.Exit(0);
                        cancel.Cancel();
                        break;
                    }

                    stream.WriteLine(data.send);
                }
            }, cancel.Token);
        }
    }

    public string receive()
    {
        string buffer = null;
        if (this.sw)
        {
            buffer = data.receive;
            data.receive = "receive fail !!";
        }
        return buffer;
    }

    public void send(string data)
    {
        if (this.sw)
        {
            this.data.send = data;
        }
    }

    public void CloseSerial()
    {
        if (this.sw)
        {
            stream.Close();
        }
    }
}
