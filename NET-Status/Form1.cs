using System.Text;


#nullable enable
namespace NET_Status
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            this.InitializeComponent();
            this.Init();
        }

        private async void Init()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            ICMPTest.Start("google.com");
            IPConfigParser.StartParsingAsync();
            DNSTest.StartTestAsync("d0bc0bb8f727.sn.mynetname.net");
            DeviceManagerParser.StartParsingAsync();
            while (true)
                await Task.Delay(1000);
        }

        private void Form1_Shown(
#nullable enable
        object sender, EventArgs e) => this.Hide();
    }
}