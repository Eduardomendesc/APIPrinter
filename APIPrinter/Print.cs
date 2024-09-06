namespace APIPrinter
{
    public class Print
    {
        public string ConnectionType { get; set; } = "1";
        public string Port { get; set; }
        public string Adress { get; set; }
        public string Portname { get; set; } = "COM5";
        public string FilePath { get; set; } = "\\computer\\printer";
        public string Printcommand { get; set; }
        public int BaudRate { get; set; } = 115200;
        public int Encoding { get; set; } = 857;
    }
}
