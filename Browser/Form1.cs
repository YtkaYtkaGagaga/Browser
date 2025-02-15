﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Browser
{
    public partial class browserForm : Form
    {
        int i = 0; //индекс вкладки
        bool isPageCompleted = false;
        List<bookmark> bookmarks;
        List<history> histories;
        FileStream streamHtml;
        string lastOpenUrl;
        int indexTabBookmark = -1;
        int indexTabHistory = -1;
        void ReadBookmarks()
        {
            StreamReader reader = new StreamReader("Bookmarks.txt");
            string fileString = reader.ReadToEnd();
            string[] fileData = fileString.Split('\n');
            bookmarks = new List<bookmark>(fileData.Length/2);
            for (int i = 0; i < fileData.Length - 2; i += 2)
            {
                bookmarks.Add(new bookmark(fileData[i], fileData[i + 1]));
            }
            reader.Close();
        }
        void ReadHistory()
        {
            StreamReader reader = new StreamReader("History.txt");
            string fileString = reader.ReadToEnd();
            string[] fileData = fileString.Split('\n');
            histories = new List<history>(fileData.Length / 3);
            for (int i = 0; i < fileData.Length - 3; i += 3)
            {
                DateTime timeBrowse = Convert.ToDateTime(fileData[i]);
                if (timeBrowse.AddDays(7)>=DateTime.Now)
                histories.Add(new history(timeBrowse, fileData[i+1], fileData[i + 2]));
            }
            reader.Close();
        }
        void SaveBookmarks()
        {
            StreamWriter writer = new StreamWriter("Bookmarks.txt");
            foreach (bookmark book in bookmarks)
            {
                writer.Write(book.name+ '\n'+book.url+'\n');
            }
            writer.Close();
        }
        void SaveHistory()
        {
            StreamWriter writer = new StreamWriter("History.txt");
            foreach (history book in histories)
            {
                writer.Write(book.time.ToString() + '\n' + book.name + '\n'+ book.url + '\n');
            }
            writer.Close();
        }
        private void HtmlDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            streamHtml.Close();
            tabControl1.TabPages[((page)sender).indexPage].Text = ((page)sender).DocumentTitle;
            if (tabControl1.SelectedIndex == indexTabHistory) 
                pageCompleted(true);
            if (((WebBrowser)sender).Url.ToString()!= "about:blank")
            {
                ((WebBrowser)sender).DocumentCompleted -= HtmlDocumentCompleted;
                ((WebBrowser)sender).DocumentCompleted += DocumentCompleted;
                if (((page)sender).indexPage == indexTabHistory)
                    indexTabHistory = -1;
            }
        }
        void CreateHtmlHistory()
        {
            try
            {
                StreamWriter streamwriter = new StreamWriter("lastHistory.html");
                streamwriter.WriteLine("<html>");
                streamwriter.WriteLine("<head>");
                streamwriter.WriteLine("  <title>История</title>");
                streamwriter.WriteLine("  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\" />");
                streamwriter.WriteLine("</head>");
                streamwriter.WriteLine("<body>");
                if (histories.Count == 0)
                {
                    streamwriter.WriteLine("<p><img src =\"https://cdn.iconscout.com/icon/premium/png-512-thumb/janitor-1631013-1380608.png\" alt=\"Clear\"></p>");
                    streamwriter.WriteLine("<p><a> Тут чисто </a></p>");
                }
                else
                    streamwriter.WriteLine("<p><img src =\"http://cdn.onlinewebfonts.com/svg/download_359214.png\" height =50 width = 50 align=\"middle\"></p>");
                foreach (history story in histories)
                {
                    streamwriter.WriteLine("<p><a>" + story.time.ToString() + "</a> <a href=" + story.url + ">" + story.name + " " + story.url + "</a></p>");
                }
                streamwriter.WriteLine("</body>");
                streamwriter.WriteLine("</html>");
                streamwriter.Close();
            }
            catch
            {

            }
        }
        public browserForm()
        {
            InitializeComponent();
            AddTab();
            ReadBookmarks();
            ReadHistory();
        }
        void AddTab()
        {
            AddTab("Yandex.ru");
        }

        void OpenHtml(string nameFile)
        {
            streamHtml = new FileStream(nameFile, FileMode.Open);
            ((WebBrowser)tabControl1.SelectedTab.Controls[0]).DocumentStream = streamHtml;
        }
        void AddHtmlTab(string nameFile)
        {
            page web = new page(i);
            web.NewWindow += NewWindow;
            tabControl1.TabPages.Add("История");
            tabControl1.TabPages[i].Controls.Add(web);
            tabControl1.SelectTab(i);
            i += 1;
            web.DocumentCompleted += HtmlDocumentCompleted;
            streamHtml = new FileStream(nameFile, FileMode.Open);
            web.DocumentStream = streamHtml;
        }
        void AddTab(string url)
        {
            page web = new page(i);
            web.DocumentCompleted += DocumentCompleted;
            web.NewWindow += NewWindow;
            tabControl1.TabPages.Add("New Pages");
            tabControl1.TabPages[i].Controls.Add(web);
            tabControl1.SelectTab(i);
            i += 1;
            web.Navigate(url);
        }
        private void NewWindow(object sender, CancelEventArgs e)
        {
            HtmlElement link = ((WebBrowser)sender).Document.ActiveElement;
            string url = link.GetAttribute("href");
            ((page)sender).OpenUrl(url);
            e.Cancel = true;
        }
        void SaveInHistory(string thisUrl, int namePage)
        {
            try
            {
                if (thisUrl != "about:blank")
                {
                    history nowOpen = new history(DateTime.Now, Convert.ToString(namePage), thisUrl);
                    try
                    {
                        if (nowOpen.url != lastOpenUrl)
                        {
                            histories.Add(nowOpen);
                        }
                    }
                    catch
                    {
                        histories.Add(nowOpen);
                    }
                    lastOpenUrl = nowOpen.url;
                }
            }
            catch
            {

            }
        }
        private void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            isPageCompleted = true;
            string namePage = ((WebBrowser)sender).DocumentTitle;
            if (namePage != "Не удается открыть эту страницу")
            {
                if (tabControl1.SelectedIndex == ((page)sender).indexPage)
                    pageCompleted(true);
                tabControl1.TabPages[((page)sender).indexPage].Text = ((page)sender).DocumentTitle;
                SaveInHistory(((page)sender).Url.ToString(),((page)sender).indexPage);
            }
            else
            {
                ((page)sender).OpenUrl("?" + ((WebBrowser)sender).Url.ToString().Split('/')[2]);
            }
        }
        private void refreshButton_Click(object sender, EventArgs e)
        {
            if (isPageCompleted)
            {
                pageCompleted(false);
                RefreshPage();
            }
            else
            {
                try
                {
                    pageCompleted(true);
                    ((WebBrowser)tabControl1.SelectedTab.Controls[0]).Stop();
                }
                catch
                {

                }
            }
        }

        void RefreshPage()
        {
            try
            {
                string url = ((WebBrowser)tabControl1.SelectedTab.Controls[0]).Url.ToString();
                if (url != "about:blank")
                {
                    string newUrl = url;
                    ((page)tabControl1.SelectedTab.Controls[0]).OpenUrl(newUrl);
                }
                else
                {
                    CreateHtmlHistory();
                    OpenHtml("lastHistory.html");
                }
            }
            catch
            {
                RefreshBookmarksTab();
                tabControl1.SelectedTab.Text = "Закладки";
                pageCompleted(true);
            }
        }
        private void backButton_Click(object sender, EventArgs e)
        {
            try
            {
                ((page)tabControl1.SelectedTab.Controls[0]).GoBack();
            }
            catch
            {

            }
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            try
            {
                ((page)tabControl1.SelectedTab.Controls[0]).GoForward();
            }
            catch
            {

            }
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            AddTab();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count > 1)
            {
                int number = tabControl1.SelectedIndex;
                if (number == indexTabBookmark)
                {
                    indexTabBookmark = -1;
                }
                if (number == indexTabHistory)
                {
                    indexTabHistory = -1;
                }
                for (int p = tabControl1.SelectedIndex; p<tabControl1.TabPages.Count - 1; p++)
                {
                    ((page)tabControl1.SelectedTab.Controls[0]).indexPage--;
                }
                int indexSeleсt = tabControl1.SelectedIndex;
                if (indexTabHistory > indexSeleсt)
                    indexTabHistory--;
                if (indexTabBookmark > indexSeleсt)
                    indexTabBookmark--;
                tabControl1.SelectedTab.Controls[0].Dispose();
                tabControl1.TabPages.RemoveAt(number);
                try
                {
                    tabControl1.SelectTab(number - 1);
                }
                catch
                {
                    tabControl1.SelectTab(0);
                }
                i -= 1;
            }
        }

        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    ((page)tabControl1.SelectedTab.Controls[0]).Navigate(richTextBox1.Text);
                }
                catch
                {
                    OpenFromBookmark(richTextBox1.Text);
                }
            }
        }

        void OpenFromBookmark(string url)
        {
            indexTabBookmark = -1;
            tabControl1.SelectedTab.Controls.Clear();
            page web = new page(tabControl1.SelectedIndex);
            web.DocumentCompleted += DocumentCompleted;
            web.NewWindow += NewWindow;
            tabControl1.SelectedTab.Controls.Add(web);
            web.Navigate(url);
        }
        private void browserForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveBookmarks();
            SaveHistory();
        }

        private void addBookmarkButton_Click(object sender, EventArgs e)
        {
            try
            {
                string newUrl = ((WebBrowser)tabControl1.SelectedTab.Controls[0]).Url.ToString();
                if (tabControl1.SelectedTab.Text != "" && tabControl1.SelectedTab.Text != "Закладки" && tabControl1.SelectedTab.Text != "История")
                {
                    bool haveBookmark = false;
                    foreach (bookmark mark in bookmarks)
                    {
                        if (mark.url.Split('/')[2] == "google.com")
                        {
                            if (mark.url.Split('/')[2] == newUrl.Split('/')[2])
                            {
                                haveBookmark = true;
                                break;
                            }
                        }
                        else
                        {
                            if (mark.url == newUrl)
                            {
                                haveBookmark = true;
                                break;
                            }
                        }
                    }
                    if (!haveBookmark)
                    {
                        bookmarks.Add(new bookmark(tabControl1.SelectedTab.Text, newUrl));
                    }
                }
            }
            catch
            {

            }
        }
        private void OpenBookmark(object sender, EventArgs e)
        {
            OpenFromBookmark(((urlButton)sender).url);
        }
        public void DeleteBookmark(object sender, EventArgs e)
        {
            foreach (bookmark mark in bookmarks)
            {
                if (mark.url == ((urlButton)sender).url)
                {
                    bookmarks.Remove(mark);
                    break;
                }
            }
            RefreshBookmarksTab();
        }

        private void richTextBox1_Enter(object sender, EventArgs e)
        {
            try
            {
                richTextBox1.Text = ((WebBrowser)tabControl1.SelectedTab.Controls[0]).Url.ToString();
                if (richTextBox1.Text == "about:blank")
                    richTextBox1.Text = "История";
            }
            catch
            {
                richTextBox1.Text = "Закладки";
            }
        }

        private void richTextBox1_Leave(object sender, EventArgs e)
        {
            richTextBox1.Text = "";
        }

        private void bookmarksButton_Click(object sender, EventArgs e)
        {
            AddBookmarksTab();
        }

        private void historyButton_Click(object sender, EventArgs e)
        {
            if (indexTabHistory == -1)
            {
                CreateHtmlHistory();
                indexTabHistory = i;
                AddHtmlTab("lastHistory.html");
            }
            else
            {
                tabControl1.SelectedIndex = indexTabHistory;
                RefreshPage();
            }
        }

        private void clearHistoryButton_Click(object sender, EventArgs e)
        {
            DialogResult mes = MessageBox.Show("Очистить историю?", 
                "История просмотров будет удалена", MessageBoxButtons.OKCancel);
            if (mes == DialogResult.OK)
            {
                histories.Clear(); 
                if (tabControl1.SelectedTab.Text == "История")
                {
                    CreateHtmlHistory();
                    OpenHtml("lastHistory.html");
                } 
            }
        }

        void AddBookmarksTab()
        {
            if (indexTabBookmark ==-1)
            {
                tabControl1.TabPages.Add("Закладки");
                tabControl1.SelectTab(i);
                CreateBookmarksTab();
                indexTabBookmark = i;
                i += 1;
            }
            else
            {
                tabControl1.SelectedIndex = indexTabBookmark;
                RefreshPage();
            }
        }
        void CreateBookmarksTab()
        {
            TableLayoutPanel mainPanel = new TableLayoutPanel();
            mainPanel.Dock = DockStyle.Fill;
            mainPanel.ColumnCount = 0;
            mainPanel.RowCount = 0;
            int column;
            int row;
            if (bookmarks.Count <= 4)
            {
                column = bookmarks.Count;
                row = 1;
            }
            else if (bookmarks.Count<=8)
            {
                column = 4;
                row = 2;
            }
            else if (bookmarks.Count <= 16)
            {
                column = 4;
                row = 4;
            }
            else
            {
                column = 8;
                row = bookmarks.Count / 8;
                if (bookmarks.Count % 8 != 0)
                    row++;
            }
            int number = 0;
            foreach (bookmark mark in bookmarks)
            {
                bookmarkButton newButton = new bookmarkButton(mark);
                newButton.openUrlButton.Click += OpenBookmark;
                newButton.deleteButton.Click += DeleteBookmark;
                if (mainPanel.ColumnCount < column)
                {
                    mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent));
                }

                else if (mainPanel.RowCount < row)
                    mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent));
                mainPanel.Controls.Add(newButton, number % column, number / column);
                number++;
            }
            foreach (ColumnStyle style in mainPanel.ColumnStyles)
            {
                style.Width = 100;
            }
            foreach (RowStyle style in mainPanel.RowStyles)
            {
                style.Height = 100;
            }
            mainPanel.Visible = true;
            if (bookmarks.Count > 0)
                mainPanel.BackColor = Color.DarkCyan;
            else 
                mainPanel.BackgroundImage = Properties.Resources.bookmarks;
            mainPanel.BackgroundImageLayout = ImageLayout.Zoom;
            tabControl1.SelectedTab.Controls.Add(mainPanel);
        }
        void RefreshBookmarksTab()
        {
            tabControl1.SelectedTab.Controls.RemoveAt(0);
            CreateBookmarksTab();

        }
        private void printButton_Click(object sender, EventArgs e)
        {
            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ((WebBrowser)tabControl1.SelectedTab.Controls[0]).Print();
                }
                catch (Exception)
                {
                    MessageBox.Show("Ошибка параметров печати.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawString(((WebBrowser)tabControl1.SelectedTab.Controls[0]).DocumentText, richTextBox1.Font, Brushes.Black, 0, 0); //Класс Graphics предоставляет методы рисования на устройстве отображения
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (tabControl1.SelectedTab.Text != "История")
                {
                    ((WebBrowser)tabControl1.SelectedTab.Controls[0]).ShowSaveAsDialog();
                }
            }
            catch
            {

            }
        }

        private void richTextBox1_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
        }
        private void pageCompleted(bool completed)
        {
            if (completed)
            {
                refreshButton.BackgroundImage = Properties.Resources.refresh;
                isPageCompleted = true;
            }
            else
            {
                refreshButton.BackgroundImage = Properties.Resources.stop;
                isPageCompleted = false;
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                bool compl = ((page)tabControl1.SelectedTab.Controls[0]).isPageCompleted;
                if (compl)
                {
                    pageCompleted(true);
                }
                else
                {
                    pageCompleted(false);
                }
            }
            catch
            {

            }
                
        }
    }
}