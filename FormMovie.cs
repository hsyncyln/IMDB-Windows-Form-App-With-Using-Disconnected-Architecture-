using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Imdb
{
    public partial class FormMovie : Form
    {
        public Movie _Movie { get; set; }
       
       // public SqlDataAdapter daMovie { get; set; }
       // public DataSet dsMovie

        public FormMovie()
        {           
            InitializeComponent();
        }

        private void FormMovie_Load(object sender, EventArgs e)
        {          
            pbxPoster.SizeMode = PictureBoxSizeMode.StretchImage;
            this.Text = _Movie.Name;

            rchbxRate.Text = _Movie.Rate.ToString();
            rchbxDescription.Text = _Movie.Description;
            rchbxDate.Text = _Movie.Date.ToString();

            if (_Movie.Poster != null) pbxPoster.Load(_Movie.Poster);
            else pbxPoster.Load("https://us.123rf.com/450wm/pavelstasevich/pavelstasevich1811/pavelstasevich181101065/112815953-stock-vector-no-image-available-icon-flat-vector.jpg?ver=6");

            SqlConnection cnn = new SqlConnection(@"server=.\MSSQLServer01;database=Imdb;trusted_connection=true");

            SqlDataAdapter da = new SqlDataAdapter("select * from MovieCastMapping", cnn);

            DataSet dtMovie = new DataSet();

            da.Fill(dtMovie);

            SqlDataAdapter da1 = new SqlDataAdapter("select * from Person", cnn);

            DataSet ds = new DataSet();

            da1.Fill(ds);

            bool hasMap = false;

            foreach (DataRow row in dtMovie.Tables[0].Rows)
            {
                if (row[dtMovie.Tables[0].Columns.IndexOf("MovieID")].ToString()==_Movie.ImdbId)
                {
                    hasMap = true;
                    
                    foreach (DataRow rowCast in ds.Tables[0].Rows)
                    {
                        if (row[dtMovie.Tables[0].Columns.IndexOf("PersonID")].ToString() == rowCast[ds.Tables[0].Columns.IndexOf("PersonID")].ToString())
                        {
                            Cast cast = new Cast();
                            cast.FirstName = rowCast[ds.Tables[0].Columns.IndexOf("FirstName")].ToString();
                            cast.LastName = rowCast[ds.Tables[0].Columns.IndexOf("LastName")].ToString();
                            cast.CastId = rowCast[ds.Tables[0].Columns.IndexOf("PersonID")].ToString();
                            cast.Description = rowCast[ds.Tables[0].Columns.IndexOf("Description")].ToString();
                            cast.PosterLink = rowCast[ds.Tables[0].Columns.IndexOf("PosterLink")].ToString();
                            cast.BirthDate = Convert.ToDateTime(rowCast[ds.Tables[0].Columns.IndexOf("BirthDate")]);
                            cast.City = rowCast[ds.Tables[0].Columns.IndexOf("City")].ToString();
                            cast.State = rowCast[ds.Tables[0].Columns.IndexOf("State")].ToString();
                            cast.Country = rowCast[ds.Tables[0].Columns.IndexOf("Country")].ToString();

                            SqlDataAdapter dar = new SqlDataAdapter("select * from Role", cnn);

                            DataTable dtMovieRole = new DataTable();
                            dar.Fill(dtMovieRole);

                            foreach(DataRow rowRole in dtMovieRole.Rows)
                            {
                                if(row[dtMovie.Tables[0].Columns.IndexOf("RoleID")].ToString()== rowRole[dtMovieRole.Columns.IndexOf("RoleID")].ToString())
                                {
                                    
                                    if (rowRole[dtMovieRole.Columns.IndexOf("RoleName")].ToString().Replace(" ","").ToLower() == "director") lstbxDirector.Items.Add(cast);
                                    else if (rowRole[dtMovieRole.Columns.IndexOf("RoleName")].ToString().Replace(" ", "").ToLower() == "writer") lstbxWriter.Items.Add(cast);
                                    else if(rowRole[dtMovieRole.Columns.IndexOf("RoleName")].ToString().Replace(" ", "").ToLower() == "star") lstbxStar.Items.Add(cast);
                                }
                            }

                            
                        }
                    }
                    
                }
            }
            if(hasMap==false)
            {
                _Movie.FindCast();

                foreach (Cast cast in _Movie.Casts)
                {
                    bool isthere = false;
                    foreach(DataRow row in dtMovie.Tables[0].Rows)
                    {
                        if (row[dtMovie.Tables[0].Columns.IndexOf("PersonID")].ToString() == cast.CastId)
                        {
                            isthere = true;
                        }
                    }
                    if (isthere == false)
                    {
                        cast.GetInfo();
                        DataRow rowPerson = ds.Tables[0].NewRow();
                        rowPerson[ds.Tables[0].Columns.IndexOf("FirstName")] = cast.FirstName;
                        rowPerson[ds.Tables[0].Columns.IndexOf("LastName")] = cast.LastName;
                        rowPerson[ds.Tables[0].Columns.IndexOf("PersonID")] = cast.CastId;
                        rowPerson[ds.Tables[0].Columns.IndexOf("Description")] = cast.Description;
                        rowPerson[ds.Tables[0].Columns.IndexOf("PosterLink")] = cast.PosterLink;
                        rowPerson[ds.Tables[0].Columns.IndexOf("BirthDate")] = cast.BirthDate;
                        rowPerson[ds.Tables[0].Columns.IndexOf("City")] = cast.City;
                        rowPerson[ds.Tables[0].Columns.IndexOf("State")] = cast.State;
                        rowPerson[ds.Tables[0].Columns.IndexOf("Country")] = cast.Country;

                        ds.Tables[0].Rows.Add(rowPerson);
                    }

                    //if (!dtMovie.Tables[0].Rows.Contains(_Movie.ImdbId))
                    //{

                        foreach (Role role in cast.Roles)
                        {
                            DataRow row = dtMovie.Tables[0].NewRow();
                            row[dtMovie.Tables[0].Columns.IndexOf("PersonID")] = cast.CastId;
                            row[dtMovie.Tables[0].Columns.IndexOf("MovieID")] = _Movie.ImdbId;

                            if (role.ToString() == "director")
                            {
                                lstbxDirector.Items.Add(cast);
                                row[dtMovie.Tables[0].Columns.IndexOf("RoleID")] = 1;
                            }
                            else if (role.ToString() == "writer")
                            {
                                lstbxWriter.Items.Add(cast);
                                row[dtMovie.Tables[0].Columns.IndexOf("RoleID")] = 2;
                            }
                            else if (role.ToString() == "star")
                            {
                                lstbxStar.Items.Add(cast);
                                row[dtMovie.Tables[0].Columns.IndexOf("RoleID")] = 3;
                            }
                            else row[dtMovie.Tables[0].Columns.IndexOf("RoleID")] = 4;

                            
                            dtMovie.Tables[0].Rows.Add(row);
                        }
                        new SqlCommandBuilder(da1);
                        da1.Update(ds);
                        new SqlCommandBuilder(da);
                        da.Update(dtMovie);
                        //   }

                }
            }
            cnn.Close();

        }
        private void LstbxStar_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            Cast cast = (Cast)lstbxStar.SelectedItem;
            FormCast form = new FormCast();
            form._Cast = cast;
            form.Show();
            
            
        }
        private void LstbxWriter_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            Cast cast = (Cast)lstbxWriter.SelectedItem;

            FormCast form = new FormCast();
            form._Cast = cast;
            form.Show();

        }
        private void LstbxDirector_SelectedIndexChanged(object sender, EventArgs e)
        {
            Cast cast = (Cast)lstbxDirector.SelectedItem;

            FormCast form = new FormCast();
            form._Cast = cast;

            form.Show();

        }

        private void TerminateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BtnX_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
