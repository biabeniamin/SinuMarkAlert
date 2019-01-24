using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;

namespace SinuChecker
{
    public partial class Form1 : Form
    {
        private string lastNotes;
        public Form1()
        {
            InitializeComponent();
        }

        private string GetPostResponse(string url, string postData)
        {
            WebRequest Logingrequest = WebRequest.Create(url);
            Logingrequest.Credentials = CredentialCache.DefaultCredentials;
            Logingrequest.ContentType = "application/x-www-form-urlencoded";
            Logingrequest.Method = "POST";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            // Set the ContentLength property of the WebRequest.
            Logingrequest.ContentLength = byteArray.Length;
            Stream dataStream = Logingrequest.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            WebResponse response = Logingrequest.GetResponse();
            dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            dataStream.Close();
            response.Close();

            return responseFromServer;
        }

        private int SearchInString(string text, string searchedString)
        {

            for (int i = 0; i < text.Length; i++)
            {
                for (int j = 0; j < searchedString.Length; j++)
                {
                    if (searchedString[j] != text[i + j])
                    {
                        break;
                    }
                    else if ((searchedString.Length - 1) == j)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private string ParseSessionId(string postResponse)
        {
            string searchedString = "name=\"sid\" value=\"";
            int searchedLocation = SearchInString(postResponse, searchedString);
            if(-1 == searchedLocation)
            {
                return "";
            }

            string beginningOfSid = postResponse.Substring(searchedLocation + searchedString.Length);

            string sid = beginningOfSid.Substring(0, SearchInString(beginningOfSid, "\""));

            return sid;
        }

        private List<string> GetTags(string text)
        {
            List<string> tags = new List<string>(new string[] { "&nbsp;" });

            for (int i = 0; i < text.Length; i++)
            {
                if('<' == text[i])
                {
                    for (int j = i + 1; j < text.Length; j++)
                    {
                        if('>' == text[j])
                        {
                            string tag = text.Substring(i, j - i + 1);
                            if (!tags.Contains(tag))
                            {
                                tags.Add(tag);
                            }
                            i = j;
                            break;
                        }
                    }
                }
            }

            return tags;
        }

        private string RemoveTags(string text)
        {
            List<string> tags = GetTags(text);

            text = text.Replace("</strong></td></tr><table>", "\n");
            text = text.Replace("Nota sau calificativul", "\n");

            foreach (string tag in tags)
            {
                text = text.Replace(tag, "");
            }

            while(true)
            {
                string notaAndDate;
                int index = SearchInString(text, "Nota");
                if(-1 == index)
                {
                    break;
                }

                notaAndDate = text.Substring(index, 14);
                text = text.Replace(notaAndDate, "");
            }



            return text;
        }

        private string GetNote()
        {
            string getSessionId = GetPostResponse("https://sinu.utcluj.ro/note/default.asp", "username=username&password=password");
            string sessionId = ParseSessionId(getSessionId);
            if(0 == sessionId.Length)
            {
                return "";
            }

            string pageResponse = GetPostResponse("https://sinu.utcluj.ro/note/roluri.asp", $"sid={sessionId}");
            pageResponse = GetPostResponse("https://sinu.utcluj.ro/note/roluri.asp", $"sid={sessionId}&hidNume_Facultate=Facultatea+de+Automatica+si+Calculatoare&hidNume_Specializare=Automatica+si+Informatica+Aplicata-lic.&hidOperation=N&hidSelfSubmit=roluri.asp");

            int searchedLocation = SearchInString(pageResponse, "Nota sau calificativul");
            if(-1 == searchedLocation)
            {
                return "";
            }

            pageResponse = pageResponse.Substring(searchedLocation);

            searchedLocation = SearchInString(pageResponse, "images/deconectare_over.gif");
            if (-1 == searchedLocation)
            {
                return "";
            }

            pageResponse = pageResponse.Substring(0,searchedLocation);

            //Materie.ParseHtml(pageResponse);

            return RemoveTags(pageResponse);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Tick += Timer1_Tick;
            timer1.Start();

            timer2.Tick += Timer2_Tick;

            
        }

        private void Timer2_Tick(object sender, EventArgs e)
        {
            if (38 == System.DateTime.Now.Minute
                    || 5 == System.DateTime.Now.Minute)
            {
                while (true)
                {
                    try
                    {
                        Check();
                        break;
                    }
                    catch (Exception ee)
                    {
                        Log(ee.Message);
                        continue;
                    }
                }

            }
        }

        private void Log(string text)
        {
            System.Console.WriteLine(text);

            StreamReader reader = new StreamReader("log.txt");
            string textInFile = reader.ReadToEnd();
            reader.Close();

            StreamWriter writer = new StreamWriter("log.txt");
            writer.Write($"{textInFile} \n\n------------------------------------------------\n\n{DateTime.Now.ToString()} : {text} \n");
            writer.Close();
        }

        public void Check()
        {
            string note = GetNote();
            Log(note);

            Properties.Settings.Default.LastData =  note;
            Properties.Settings.Default.Save();

            if (note != lastNotes)
            {
                axWindowsMediaPlayer1.URL = @"D:\Beni\C#\SinuChecker\SinuChecker\bin\Debug\bell.wav";
                axWindowsMediaPlayer1.Ctlcontrols.play();
                axWindowsMediaPlayer1.settings.setMode("loop", true);
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            this.Hide();
            timer1.Stop();


            lastNotes = Properties.Settings.Default.LastData;
            Check();

            timer2.Start();
            Log(lastNotes);

        }
    }
}
