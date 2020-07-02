using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Windows.Forms;

namespace Imdb
{
    public partial class FormMain : Form
    {
        List<Movie> _listmovies;
        DataSet dtMovie;
        SqlDataAdapter daMovie;


        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            SqlConnection cnn = new SqlConnection(@"server=.\MSSQLServer01;database=IMDB;trusted_connection=true");

            dtMovie = new DataSet();

            daMovie = new SqlDataAdapter("select * from Movie", cnn);
            daMovie.Fill(dtMovie);

            cnn.Close();
            
        }
        public void Findmovie(string txt)  //film isimlerini ve urllerini listeye atar
        {
            WebClient webClient = new WebClient();
            string result = webClient.DownloadString("https://www.imdb.com/find?q=" + txt);

            string movieName,movieUrl;

            int startsWith = result.IndexOf("table");     //filmler <table> ların arasında olduğundan kontrol edilecek data azaltıldı
            int endsWith = result.IndexOf("/table");
            string movie_table = result.Substring(startsWith, endsWith - startsWith);


            while (movie_table.IndexOf("result_text") != -1)  // '/title' ile başlayan kısımlar listelenmiş filmleri url leridir. 
            {                                           //Eğer -1 ise aranan filmlerin url leri alınmıştır

                startsWith = movie_table.IndexOf("/title");          //ilk url'in başlangıç noktasını bulur
                movie_table = movie_table.Substring(startsWith + 1);    //başlangıç noktasını bulunan yere taşır

                startsWith = movie_table.IndexOf("/title");          //ikinci url'in başlangıç noktasını bulur ve taşır
                movie_table = movie_table.Substring(startsWith);
                endsWith = movie_table.IndexOf('?');               // url " işaretine kadar olan kısım olduğundan son noktasını buldum

                movieUrl = movie_table.Substring(0, endsWith);  //bulduğumuz url 

                endsWith = movie_table.IndexOf(">");
                movie_table = movie_table.Substring(endsWith + 1);     //filmlerin isimlerini bulabilmek için bulduğumuz yeri çıkarıyoruz
                endsWith = movie_table.IndexOf("<");

                movieName = movie_table.Substring(0, endsWith);

                startsWith = movie_table.IndexOf(">");
                movie_table = movie_table.Substring(startsWith + 1);
                endsWith = movie_table.IndexOf("<");

                movieName += movie_table.Substring(0, endsWith);

                Movie movie = new Movie(movieName, movieUrl);  //movie tipinde bir instance olusturur
                _listmovies.Add(movie);
                
                movie_table = movie_table.Substring(endsWith + 1);
            }
            
        }

        private void Btn_search_Click(object sender, EventArgs e)
        {
            //Movie listbox ına filmleri atar
            lstbxMovie.Items.Clear();
                        
            bool control = false;

            foreach (DataRow row in dtMovie.Tables[0].Rows)
            {
                if (row[dtMovie.Tables[0].Columns.IndexOf("MovieName")].ToString().ToUpper().Contains(txtbx.Text.ToUpper()))
                {
                    control = true;

                    Movie movie=new Movie();
                    movie.Name = row[dtMovie.Tables[0].Columns.IndexOf("MovieName")].ToString();
                    movie.ImdbId=row[dtMovie.Tables[0].Columns.IndexOf("MovieID")].ToString();
                    movie.Rate= Convert.ToInt32(row[dtMovie.Tables[0].Columns.IndexOf("MovieRate")]);
                    movie.Date= Convert.ToDateTime(row[dtMovie.Tables[0].Columns.IndexOf("MovieDate")]);
                    movie.Description= row[dtMovie.Tables[0].Columns.IndexOf("Description")].ToString();
                    movie.Poster = row[dtMovie.Tables[0].Columns.IndexOf("MoviePosterLink")].ToString();

                    lstbxMovie.Items.Add(movie);

                }
            }
            if (control == false)
            {
                _listmovies = new List<Movie>();

                Findmovie(txtbx.Text);

                foreach (Movie movie in _listmovies)
                {
                    movie.GetInfo();
                    DataRow row = dtMovie.Tables[0].NewRow();
                    row[dtMovie.Tables[0].Columns.IndexOf("MovieName")] = movie.Name;
                    row[dtMovie.Tables[0].Columns.IndexOf("MovieID")] = movie.ImdbId;
                    row[dtMovie.Tables[0].Columns.IndexOf("MovieRate")] = movie.Rate;
                    row[dtMovie.Tables[0].Columns.IndexOf("MovieDate")] = movie.Date;
                    row[dtMovie.Tables[0].Columns.IndexOf("Description")] = movie.Description;
                    row[dtMovie.Tables[0].Columns.IndexOf("MoviePosterLink")] = movie.Poster;
                    dtMovie.Tables[0].Rows.Add(row);
                    lstbxMovie.Items.Add(movie);
                }
                
                new SqlCommandBuilder(daMovie);
                daMovie.Update(dtMovie.Tables[0]);
            }
        }
        
        private void Lstbx_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Movie bilgilerinin bulundugu formu acar

            Movie movie = (Movie)lstbxMovie.SelectedItem;
            FormMovie form = new FormMovie();
            form._Movie = movie;
            form.Show();
       
        }

        private void Btn_clear_Click(object sender, EventArgs e)   //ekranı temizlemek için kullanılıyor
        {
            txtbx.Clear();
            lstbxMovie.Items.Clear();
            
        }

        private void BtnX_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
