using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Medecins
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SqlConnection conDB;
        public MainWindow()
        {
            // assurez vous de changer la valeur de Source et mettre celle de votre Serveur SQL
            conDB = new SqlConnection(@"Data Source=LUAY\SQLEXPRESS;Initial Catalog=Medecins;Integrated Security=True");
            InitializeComponent();
            charger_Liste_Medecins();
            dgMedecin.SelectionChanged += Ligne_Selectionnee;
            Salaire_Superieur_A.Checked += superieur;
            Salaire_Inferieur_A.Checked += inferieur;
            cbtConsulter.Click += Salaire_Consulter;
            cbtNomPrenom.Click += consulter_Nom_Prenom;

        }


        public void charger_Liste_Medecins()
        {
            string requete = "SELECT * FROM MEDECIN;";

            // Create a SqlCommand with the SQL query and connection
            SqlCommand cmd = new SqlCommand(requete, conDB);

            // Open the database connection if it's not already open
            if (conDB.State != ConnectionState.Open)
                conDB.Open();

            // Set the CommandType to Text
            cmd.CommandType = CommandType.Text;

            // Execute the query and get a SqlDataReader
            SqlDataReader dr = cmd.ExecuteReader();

            // Create a DataTable
            DataTable dataTable = new DataTable();

            // Load the DataTable with data from the SqlDataReader
            dataTable.Load(dr);

            // Set the DataTable as the ItemsSource for the DataGrid
            dgMedecin.ItemsSource = dataTable.DefaultView;
        }

        private void Ligne_Selectionnee(object sender, SelectionChangedEventArgs e)
        {
            // Vérifiez qu'il y a une ligne sélectionnée
            if (dgMedecin.SelectedItem != null)
            {
                // Obtenez la DataRowView de la ligne sélectionnée
                DataRowView row = (DataRowView)dgMedecin.SelectedItem;

                // Remplissez vos contrôles avec les données de la ligne sélectionnée
                tbIDMedecin.Text = row["MedecinID"].ToString();
                tbnom.Text = row["Nom"].ToString();
                tbPrenom.Text = row["Prenom"].ToString();
                tbNumeroContact.Text = row["phone"].ToString();
                tbCourriel.Text = row["email"].ToString();
                tbSalaire.Text = row["Salaire"].ToString();
                tbSpecialite.Text = row["Specialite"].ToString();
                tbHopital.Text = row["Hopital"].ToString();

                // Vous pouvez également gérer d'autres actions ou mises à jour ici si nécessaire.
            }
        }


        public bool Verifier_champ()
        {

            return (!string.IsNullOrEmpty(tbPrenom.Text) && !string.IsNullOrEmpty(tbnom.Text) && !string.IsNullOrEmpty(tbNumeroContact.Text) && !string.IsNullOrEmpty(tbCourriel.Text)
                && !string.IsNullOrEmpty(tbSalaire.Text) && !string.IsNullOrEmpty(tbSpecialite.Text) && !string.IsNullOrEmpty(tbHopital.Text));
        }

        public bool Medecin_Existant(int MedID)
        {

            bool medecin_existe = false;
            // verifier si medecin existe
            //1. Si oui return true sinon return false;
            //2. gestion des excpetions try catch
            try
            {
                // Créez une commande SQL pour vérifier si le médecin existe en utilisant son ID
                string requete = "SELECT COUNT(*) FROM MEDECIN WHERE MedecinID = @MedecinID;";
                SqlCommand cmd = new SqlCommand(requete, conDB);
                cmd.Parameters.AddWithValue("@MedecinID", MedID);

                // Ouvrez la connexion si elle n'est pas déjà ouverte
                if (conDB.State != ConnectionState.Open)
                    conDB.Open();

                // Exécutez la commande SQL et vérifiez le nombre de résultats
                int count = Convert.ToInt32(cmd.ExecuteScalar());

                // Si le nombre de résultats est supérieur à zéro, le médecin existe
                medecin_existe = count > 0;
            }
            catch (Exception ex)
            {
                // Gérez les exceptions ici, affichez des messages d'erreur dans lbMessage
                lbMessage.Content = "Erreur lors de la vérification de l'existence du médecin : " + ex.Message;
            }
            finally
            {
                // Fermez la connexion après avoir effectué l'opération
                if (conDB.State == ConnectionState.Open)
                    conDB.Close();
            }

            return medecin_existe;
        }
        private void btnAjouteur_Med(object sender, RoutedEventArgs e)
        {
            // Ajouter un medecin de la BBD,
            // verifier:
            //1. Medecin n'existe pas d'abord
            //2. tous les champs sont saisis
            //3. gestion des excpetions try catch
            //4. Messages dans le status Bar
            // Vérifiez si tous les champs sont saisis
            // Vérifiez si tous les champs sont saisis
            if (Verifier_champ())
            {
                // Vérifiez si le médecin n'existe pas d'abord
                int medecinID = Convert.ToInt32(tbIDMedecin.Text);
                if (!Medecin_Existant(medecinID))
                {
                    // Ajoutez le médecin à la base de données
                    AjouterMedecin();

                    // Rafraîchissez la grille après l'ajout
                    charger_Liste_Medecins();

                    // Ajoutez ici tout autre code nécessaire ou des messages de statut.
                    lbMessage.Content = "Le médecin a été ajouté avec succès.";
                }
                else
                {
                    lbMessage.Content = "Un médecin avec cet ID existe déjà dans la base de données.";
                }
            }
            else
            {
                lbMessage.Content = "Veuillez remplir tous les champs avant d'ajouter un médecin.";
            }
        }

        // Méthode pour ajouter un médecin à la base de données
        private void AjouterMedecin()
        {
            try
            {
                // Create a SQL command to insert a new doctor excluding the MedecinID column
                string requete = "INSERT INTO MEDECIN (Prenom, Nom, phone, email, Salaire, Specialite, Hopital) " +
                                 "VALUES (@Prenom, @Nom, @phone, @email, @Salaire, @Specialite, @Hopital);";
                SqlCommand cmd = new SqlCommand(requete, conDB);

                // Add parameters for the new entry excluding MedecinID
                cmd.Parameters.AddWithValue("@Prenom", tbPrenom.Text);
                cmd.Parameters.AddWithValue("@Nom", tbnom.Text);
                cmd.Parameters.AddWithValue("@phone", tbNumeroContact.Text);
                cmd.Parameters.AddWithValue("@email", tbCourriel.Text);
                cmd.Parameters.AddWithValue("@Salaire", Convert.ToDecimal(tbSalaire.Text));
                cmd.Parameters.AddWithValue("@Specialite", tbSpecialite.Text);
                cmd.Parameters.AddWithValue("@Hopital", tbHopital.Text);

                if (conDB.State != ConnectionState.Open)
                    conDB.Open();

                // Execute the SQL command to add the doctor
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex; // Throw the exception to handle it in the caller method
            }
            finally
            {
                if (conDB.State == ConnectionState.Open)
                    conDB.Close();
            }
        }


        private void btnsuprimer_Med(object sender, RoutedEventArgs e)
        {
            // Votre code ici pour retirer un medecin de la base de donnees (BDD).
            //1. verifier si le medecin existe si oui retirez le de la BDD, sinon message pour dire qu'il n'existe pas
            // assurez vous de rafraichir la grille apres avoir retire le medecin.
            // Vérifiez s'il y a une ligne sélectionnée
            // Vérifiez s'il y a une ligne sélectionnée
            if (dgMedecin.SelectedItem != null)
            {
                // Obtenez la DataRowView de la ligne sélectionnée
                DataRowView row = (DataRowView)dgMedecin.SelectedItem;

                // Obtenez l'ID du médecin à supprimer
                int medecinIDToDelete = Convert.ToInt32(row["MedecinID"]);

                // Vérifiez si le médecin existe avant de le supprimer
                if (Medecin_Existant(medecinIDToDelete))
                {
                    // Supprimez le médecin de la base de données
                    SupprimerMedecin(medecinIDToDelete);

                    // Rafraîchissez la grille après la suppression
                    charger_Liste_Medecins();

                    // Ajoutez ici tout autre code nécessaire ou des messages de statut.
                    lbMessage.Content = "Le médecin a été supprimé avec succès.";
                }
                else
                {
                    lbMessage.Content = "Le médecin n'existe pas dans la base de données.";
                }
            }
            else
            {
                lbMessage.Content = "Veuillez sélectionner un médecin à supprimer.";
            }
        }

        // Méthode pour supprimer un médecin de la base de données
        private void SupprimerMedecin(int medecinID)
        {
            try
            {
                // Créez une commande SQL pour supprimer le médecin en utilisant son ID
                string requete = "DELETE FROM MEDECIN WHERE MedecinID = @MedecinID;";
                SqlCommand cmd = new SqlCommand(requete, conDB);
                cmd.Parameters.AddWithValue("@MedecinID", medecinID);

                // Ouvrez la connexion si elle n'est pas déjà ouverte
                if (conDB.State != ConnectionState.Open)
                    conDB.Open();

                // Exécutez la commande SQL pour supprimer le médecin
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                // Gérez les exceptions ici, affichez des messages d'erreur dans lbMessage
                lbMessage.Content = "Erreur lors de la suppression du médecin : " + ex.Message;
            }
            finally
            {
                // Fermez la connexion après avoir effectué l'opération
                if (conDB.State == ConnectionState.Open)
                    conDB.Close();
            }
        }

        private void superieur(object sender, RoutedEventArgs e)
        {
            if (Salaire_Superieur_A.IsChecked == true) Salaire_Inferieur_A.IsChecked = false;
        }

        private void inferieur(object sender, RoutedEventArgs e)
        {
            if (Salaire_Inferieur_A.IsChecked == true) Salaire_Superieur_A.IsChecked = false;
        }

        private void Salaire_Consulter(object sender, RoutedEventArgs e)
        {
            try
            {
                string condition = "";

                if (Salaire_Superieur_A.IsChecked == true)
                {
                    condition = "Salaire > @Salaire";
                }
                else if (Salaire_Inferieur_A.IsChecked == true)
                {
                    condition = "Salaire < @Salaire";
                }
                else
                {
                    condition = "Salaire = @Salaire";
                }

                string requete = "SELECT * FROM MEDECIN WHERE " + condition + ";";
                SqlCommand cmd = new SqlCommand(requete, conDB);
                cmd.Parameters.AddWithValue("@Salaire", Convert.ToDecimal(ctbSalaire.Text));

                if (conDB.State != ConnectionState.Open)
                    conDB.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                DataTable dataTable = new DataTable();
                dataTable.Load(dr);

                // Assurez-vous que la grille n'est pas liée à d'autres données
                grille_consulter.ItemsSource = null;
                grille_consulter.Columns.Clear();

                grille_consulter.AutoGenerateColumns = true;
                grille_consulter.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de la consultation par salaire : " + ex.Message);
            }
            finally
            {
                if (conDB.State == ConnectionState.Open)
                    conDB.Close();
            }
        }

        private void consulter_Nom_Prenom(object sender, RoutedEventArgs e)
        {
            // ici implementer la requete pour consultation par Nom et/ou Prenom
            // gestion des excpetions try catch
            // assurez vous d'implementer les cas possibles Nom / Prenom / Nom et Prenom / rien
            try
            {
                string nom = ctbNom.Text.Trim();
                string prenom = ctbPrenom.Text.Trim();

                string condition = "";
                SqlCommand cmd = new SqlCommand();

                if (!string.IsNullOrEmpty(nom) && !string.IsNullOrEmpty(prenom))
                {
                    condition = "Nom = @Nom AND Prenom = @Prenom";
                    cmd.Parameters.AddWithValue("@Nom", nom);
                    cmd.Parameters.AddWithValue("@Prenom", prenom);
                }
                else if (!string.IsNullOrEmpty(nom))
                {
                    condition = "Nom = @Nom";
                    cmd.Parameters.AddWithValue("@Nom", nom);
                }
                else if (!string.IsNullOrEmpty(prenom))
                {
                    condition = "Prenom = @Prenom";
                    cmd.Parameters.AddWithValue("@Prenom", prenom);
                }
                else
                {
                    MessageBox.Show("Veuillez spécifier au moins un nom ou un prénom pour la consultation.");
                    return;
                }

                string requete = "SELECT * FROM MEDECIN WHERE " + condition + ";";
                cmd.CommandText = requete;
                cmd.Connection = conDB;

                if (conDB.State != ConnectionState.Open)
                    conDB.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                DataTable dataTable = new DataTable();
                dataTable.Load(dr);

                grille_consulter.ItemsSource = null;
                grille_consulter.Columns.Clear();
                grille_consulter.AutoGenerateColumns = true;
                grille_consulter.ItemsSource = dataTable.DefaultView;
            }
            catch (Exception ex)
            {
                lbMessage.Content = "Erreur lors de la consultation : " + ex.Message;
            }
            finally
            {
                if (conDB.State == ConnectionState.Open)
                    conDB.Close();
            }

        }

        private void ModifierMedecin()
        {
            try
            {
                // Créez une commande SQL pour mettre à jour un médecin existant
                string requete = "UPDATE MEDECIN SET Prenom = @Prenom, Nom = @Nom, phone = @phone, email = @email, "
                                + "Salaire = @Salaire, Specialite = @Specialite, Hopital = @Hopital "
                                + "WHERE MedecinID = @MedecinID;";
                SqlCommand cmd = new SqlCommand(requete, conDB);

                // Ajoutez les paramètres pour la mise à jour
                cmd.Parameters.AddWithValue("@Prenom", tbPrenom.Text);
                cmd.Parameters.AddWithValue("@Nom", tbnom.Text);
                cmd.Parameters.AddWithValue("@phone", tbNumeroContact.Text);
                cmd.Parameters.AddWithValue("@email", tbCourriel.Text);
                cmd.Parameters.AddWithValue("@Salaire", Convert.ToDecimal(tbSalaire.Text));
                cmd.Parameters.AddWithValue("@Specialite", tbSpecialite.Text);
                cmd.Parameters.AddWithValue("@Hopital", tbHopital.Text);
                cmd.Parameters.AddWithValue("@MedecinID", Convert.ToInt32(tbIDMedecin.Text)); // ID du médecin à mettre à jour

                if (conDB.State != ConnectionState.Open)
                    conDB.Open();

                // Exécutez la commande SQL pour mettre à jour le médecin
                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    lbMessage.Content = "Le médecin a été modifié avec succès.";
                    charger_Liste_Medecins(); // Rafraîchissez la grille après la modification
                }
                else
                {
                    lbMessage.Content = "Aucun médecin n'a été trouvé avec cet ID.";
                }
            }
            catch (Exception ex)
            {
                lbMessage.Content = "Erreur lors de la modification du médecin : " + ex.Message;
            }
            finally
            {
                if (conDB.State == ConnectionState.Open)
                    conDB.Close();
            }
        }


        private void tbnModifier_Med(object sender, RoutedEventArgs e)
        {
            if (Verifier_champ())
            {
                try
                {
                    ModifierMedecin(); // Appel de la méthode pour modifier le médecin

                    // Ici, vous pouvez ajouter d'autres actions ou messages de statut si nécessaire.
                }
                catch (Exception ex)
                {
                    lbMessage.Content = "Erreur lors de la modification du médecin : " + ex.Message;
                }
            }
            else
            {
                lbMessage.Content = "Veuillez remplir tous les champs avant de modifier le médecin.";
            }
        }

    }
}