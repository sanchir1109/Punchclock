using System;
using System.Windows.Forms;
using Serilog;
using Microsoft.VisualBasic.Logging;
using System.Windows.Forms;

namespace irtsProject
{
    public partial class Form1 : Form
    {
        // Initialize Serilog logger
        private static readonly ILogger Logger = new LoggerConfiguration()
            .WriteTo.File("log.txt") // Specify the log file
            .CreateLogger();
        public DatabaseConnection db;

        public HuviinMedeelel hm = null;
        public TsagiinHuvaari th = null;
        public Tuuh tuuh = null;
        public Tailan tailan = null;
        public Login login = null;

        public DateTime selectedDate;
        List<DateInfo> week;
        List<DateInfo> tuuhWeek;
        List<DateInfo> tailanWeek;

        DateTime start;
        DateTime end;
        public Form1()
        {
            db = new DatabaseConnection();
            selectedDate = DateTime.Today;
            InitializeComponent();
            //this.Controls.Add(login);
        }
        /// <summary>
        /// form нээгдэх үед бүх хууласнуудын утгуудыг датабаазаас аван оноож өгнө.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            hm = new HuviinMedeelel();
            th = new TsagiinHuvaari();
            tuuh = new Tuuh();
            tailan = new Tailan();
            login = new Login();

            login.button1.Click += checkLogin;
            th.button1.Click += AjilEhleh;
            th.button2.Click += AjilDuusgah;
            tuuh.dateTimePicker1.ValueChanged += dateChanged;
            tailan.dateTimePicker1.ValueChanged += tailanDateChanged;
            tailan.dateTimePicker2.ValueChanged += tailanDateChanged;
            splitContainer1.Panel2.Controls.Add(hm);
            splitContainer1.Panel2.Controls.Add(th);
            splitContainer1.Panel2.Controls.Add(tuuh);
            splitContainer1.Panel2.Controls.Add(tailan);
            hidePages();
            userControl12.label1.Text = "Цагийн хуваарь";
            userControl11.pictureBox1.Image = Image.FromFile("C:\\Users\\user\\Downloads\\irtsProject\\IrtsProject\\user.png");
            userControl12.pictureBox1.Image = Image.FromFile("C:\\\\Users\\\\user\\\\Downloads\\\\irtsProject\\\\IrtsProject\\\\schedule.png");
            userControl13.label1.Text = "Түүх";
            userControl13.pictureBox1.Image = Image.FromFile("C:\\Users\\user\\Downloads\\irtsProject\\IrtsProject\\history.png");

            userControl14.label1.Text = "Тайлан";
            userControl14.pictureBox1.Image = Image.FromFile("C:\\Users\\user\\Downloads\\irtsProject\\IrtsProject\\tailan.png");

            hm.ner.Text = db.user.ner;
            hm.ovog.Text = db.user.ovog;
            hm.email.Text = db.user.email;
            hm.utas.Text = db.user.utas;
            hm.uureg.Text = db.user.uureg;
            hm.system_ajiltan.Text = db.user.sys_shinjeec;
            hm.suljeenii_ajiltan.Text = db.user.suljee_ajiltan;

            start = db.infos[0].date;
            end = db.infos.LastOrDefault().date;

            th.label1.Text = selectedDate.ToString();
            week = db.GetWeekInfo(selectedDate);
            tuuhWeek = db.GetWeekInfo(selectedDate);
            tailanWeek = db.getByInterval(start, end);

            renderTimeSchedule();
            renderTuuhSchedule();
            renderTailan();
            this.Controls.Add(login);
            login.BringToFront();
            login.Show();
        }
        /// <summary>
        /// Login хуудаснаас нэвтрэх дарах үед баазаас шалгалт хийж хариуг өгнө.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkLogin(object sender, EventArgs e)
        {
            if (login.textBox1.Text != null && login.textBox2.Text != null)
            {
                if (db.checkUser(login.textBox1.Text, login.textBox2.Text))
                {
                    login.Hide();
                    Logger.Information("User logged in", login.textBox1.Text);
                }
                else
                {
                    MessageBox.Show("Нэвтрэх нэр, нууц үг буруу.");
                    Logger.Warning("Login failed for user", login.textBox1.Text);
                }
            }
            else
            {
                MessageBox.Show("Мэдээлэл бүрэн оруулна уу.");
                Logger.Warning("Incomplete login information");
            }
        }
        /// <summary>
        /// тайлан хуудаснаас Date-үүдийг өөрчлөх болгонд нийт ажилласан цагуудыг харуулна.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tailanDateChanged(object sender, EventArgs e)
        {
            tailanWeek.Clear();
            start = tailan.dateTimePicker1.Value;
            end = tailan.dateTimePicker2.Value;
            tailanWeek = db.getByInterval(start, end);
            renderTailan();
        }
        /// <summary>
        /// Тайлангийн утгуудыг харуулна.
        /// </summary>
        public void renderTailan()
        {
            TimeSpan totalWorkedHours = TimeSpan.Zero;
            foreach (DateInfo i in tailanWeek)
            {
                TimeSpan startTime = TimeSpan.Parse(i.orsonTsag);
                TimeSpan endTime = TimeSpan.Parse(i.endTime);
                TimeSpan workedHours = endTime - startTime;
                totalWorkedHours += workedHours;
            }
            double totalHours = totalWorkedHours.TotalHours;
            tailan.label7.Text = totalHours.ToString();
        }
        /// <summary>
        /// Түүх хуудаснаас нэг өдөр сонгох үед тухайн долоон хонгийн утгуудыг тооцоолон гаргаж харуулна.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dateChanged(object sender, EventArgs e)
        {
            DateTime pickedDate = tuuh.dateTimePicker1.Value;
            pickedDate = new DateTime(pickedDate.Year, pickedDate.Month, pickedDate.Day, 0, 0, 0);

            Console.WriteLine(pickedDate.ToString());
            tuuhWeek.Clear();
            tuuhWeek = db.GetWeekInfo(pickedDate);
            string a = "";
            foreach (DateInfo i in tuuhWeek)
            {
                a += i.date.ToString() + i.orsonTsag.ToString() + "\n";
            }
            Console.WriteLine(a);
            renderTuuhSchedule();
        }
        /// <summary>
        /// Түүх хуудасны утгуудыг оноож өгнө.
        /// </summary>
        public void renderTuuhSchedule()
        {
            tuuh.label19.Text = tuuhWeek[0].startTime;
            tuuh.label18.Text = tuuhWeek[1].startTime;
            tuuh.label17.Text = tuuhWeek[2].startTime;
            tuuh.label16.Text = tuuhWeek[3].startTime;
            tuuh.label15.Text = tuuhWeek[4].startTime;
            tuuh.label3.Text = tuuhWeek[5].startTime;
            tuuh.label2.Text = tuuhWeek[6].startTime;

            tuuh.label26.Text = tuuhWeek[0].orsonTsag;
            tuuh.label25.Text = tuuhWeek[1].orsonTsag;
            tuuh.label24.Text = tuuhWeek[2].orsonTsag;
            tuuh.label23.Text = tuuhWeek[3].orsonTsag;
            tuuh.label22.Text = tuuhWeek[4].orsonTsag;
            tuuh.label21.Text = tuuhWeek[5].orsonTsag;
            tuuh.label20.Text = tuuhWeek[6].orsonTsag;

            tuuh.label33.Text = tuuhWeek[0].endTime;
            tuuh.label32.Text = tuuhWeek[1].endTime;
            tuuh.label31.Text = tuuhWeek[2].endTime;
            tuuh.label30.Text = tuuhWeek[3].endTime;
            tuuh.label29.Text = tuuhWeek[4].endTime;
            tuuh.label28.Text = tuuhWeek[5].endTime;
            tuuh.label27.Text = tuuhWeek[6].endTime;
        }
        /// <summary>
        /// ажил эхлүүлсэн цагийг бааз руу бүртгэн авч өөрчлөлтийг харуулна.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AjilEhleh(object sender, EventArgs e)
        {
            DateTime currentTime = DateTime.Now;
            string tsag = currentTime.ToString("HH:mm");
            string day = currentTime.ToString("yyyy/MM/dd");
            db.tsagBurtgeh(0, tsag, day);
            string format = "yyyy/MM/dd";
            DateTime dateTime = DateTime.ParseExact(day + " 12:00:00 AM", format + " hh:mm:ss tt", null);

            Console.WriteLine("match:" + dateTime.ToString());

            foreach (DateInfo i in week)
            {
                if (i.date.Equals(dateTime))
                {
                    i.orsonTsag = tsag;
                    Console.WriteLine("matches");
                }
            }
            renderTimeSchedule();
            MessageBox.Show("Цаг амжилттай бүртгэгдлээ.");
        }
        /// <summary>
        /// ажил дуугасан цагийг бааз руу бүртгэн авч өөрчлөлтийг харуулна.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AjilDuusgah(object sender, EventArgs e)
        {
            DateTime currentTime = DateTime.Now;
            string tsag = currentTime.ToString("HH:mm");
            string day = currentTime.ToString("yyyy/MM/dd");
            db.tsagBurtgeh(1, tsag, day);
            string format = "yyyy/MM/dd";
            DateTime dateTime = DateTime.ParseExact(day + " 12:00:00 AM", format + " hh:mm:ss tt", null);
            //MessageBox.Show("match:" + dateTime.ToString());
            foreach (DateInfo i in week)
            {
                if (i.date.Equals(dateTime))
                {
                    i.endTime = tsag;
                    //MessageBox.Show("matches");
                }
            }
            renderTimeSchedule();
            MessageBox.Show("Цаг амжилттай бүртгэгдлээ.");
        }
        /// <summary>
        /// Цагийн хуваарь хуудасны утгуудыг оноож харуулна.
        /// </summary>
        public void renderTimeSchedule()
        {
            //string a = "";
            //foreach(DateInfo i  in week)
            //{
            //    a += i.date.ToString()+ i.orsonTsag.ToString() + "\n";
            //}
            //MessageBox.Show(a);
            th.label21.Text = week[0].startTime;
            th.label20.Text = week[1].startTime;
            th.label19.Text = week[2].startTime;
            th.label18.Text = week[3].startTime;
            th.label17.Text = week[4].startTime;
            th.label16.Text = week[5].startTime;
            th.label15.Text = week[6].startTime;

            th.label28.Text = week[0].orsonTsag;
            th.label27.Text = week[1].orsonTsag;
            th.label26.Text = week[2].orsonTsag;
            th.label25.Text = week[3].orsonTsag;
            th.label24.Text = week[4].orsonTsag;
            th.label23.Text = week[5].orsonTsag;
            th.label22.Text = week[6].orsonTsag;

            th.label35.Text = week[0].endTime;
            th.label34.Text = week[1].endTime;
            th.label33.Text = week[2].endTime;
            th.label32.Text = week[3].endTime;
            th.label31.Text = week[4].endTime;
            th.label30.Text = week[5].endTime;
            th.label29.Text = week[6].endTime;

        }
        /// <summary>
        /// бүх хуудсыг нууна.
        /// </summary>
        public void hidePages()
        {
            hm.Hide();
            th.Hide();
            tuuh.Hide();
            tailan.Hide();
        }
        private void splitContainer1_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }
        /// <summary>
        /// Бүх хуудсыг нууж, хувийн мэдээлэл хуудсыг харуулна.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showHuvinMedeelel(object sender, EventArgs e)
        {
            hidePages();
            hm.Show();
        }
        /// <summary>
        /// Бүх хуудсыг нууж, цагийн хуваарь хуудсыг харуулна.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showTsaginHuvaari(object sender, EventArgs e)
        {
            hidePages();
            th.Show();
        }
        /// <summary>
        /// Бүх хуудсыг нууж, Түүх хуудсыг харуулна.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showTuuh(object sender, EventArgs e)
        {
            hidePages();
            renderTuuhSchedule();
            tuuh.Show();
        }
        /// <summary>
        /// Бүх хуудсыг нууж, Тайлан хуудсыг харуулна.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showTailan(object sender, EventArgs e)
        {
            hidePages();
            tailan.Show();
        }
        /// <summary>
        /// Гарах буюу Login хуудсыг харуулна.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            login.textBox1.Text = "";
            login.textBox2.Text = "";
            login.Show();
        }
    }
}