using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using static Xamarin.Forms.Internals.GIFBitmap;

namespace HEC_Mobile
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            ReadRecipesFromFile();
            RecipesList.ItemsSource = Recipe.RecipesToShow;
            Recipe.AllRecipes.Add(new Recipe("KJifshvbg", "siudhf", new Guid(), new List<Ingredience>()));
            Recipe.AllRecipes.Add(new Recipe("KJifshg", "sdhf", new Guid(), new List<Ingredience>()));
            Recipe.AllRecipes.Add(new Recipe("KJifvbg", "sihf", new Guid(), new List<Ingredience>()));
            Recipe.AllRecipes.Add(new Recipe("KJshvbg", "siud", new Guid(), new List<Ingredience>()));
            Recipe.UpdateToMatchFilter(new Recipe() { Search = SearchEntry.Text });
            UpdateRecipeListview();
        }

        public void UpdateRecipeListview()
        {
            RecipesList.ItemsSource = null;
            RecipesList.ItemsSource = Recipe.RecipesToShow;
        }

        public static void WriteRecipesToFile()
        {
            using (StreamWriter sw = new StreamWriter(@".\Recipes.txt"))
            {
                foreach (var item in Recipe.AllRecipes)
                {
                    sw.WriteLine(Recipe.SerializeRecipe(item));
                }
            }
        }
        public void ReadRecipesFromFile()
        {
            try
            {
                string[] lines = File.ReadAllLines(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Recipes.txt"));
                foreach (var item in lines)
                {
                    Recipe.AllRecipes.Add(Recipe.DeserializeRecipe(item));
                }
                UpdateRecipeListview();
            }
            catch (Exception) { }
        }

        private void BtnReadClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            var index = Recipe.AllRecipes.IndexOf((Recipe)button.BindingContext);
            SearchEntry.Text = index.ToString();
            Navigation.PushAsync(new ReadRecipe(index));
            UpdateRecipeListview();
        }
        private void BtnEditClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            var index = Recipe.AllRecipes.IndexOf((Recipe)button.BindingContext);
            SearchEntry.Text = index.ToString();
            Navigation.PushAsync(new EditRecipe(index));
            UpdateRecipeListview();
        }
        private async void BtnDeleteClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Recipe recipe = (Recipe)button.BindingContext;
            if (await DisplayAlert("Delete Recipe", $"Do you want to delete {recipe.Name}?", "Yes", "No"))
            {
                Recipe.RecipesToShow.Remove(recipe);
                UpdateRecipeListview();
            }
        }
        private void BtnNewRecipeClicked(object sender, EventArgs e)
        {

        }
        private void BtnImportClicked(object sender, EventArgs e)
        {

        }
        private void BtnExportClicked(object sender, EventArgs e)
        {

        }
        private void BtnRandomClicked(object sender, EventArgs e)
        {

        }
        private void BtnSearchClicked(object sender, EventArgs e)
        {
            Recipe.UpdateToMatchFilter(new Recipe() { Search = SearchEntry.Text});
            UpdateRecipeListview();
        }
    }
    class Recipe : INotifyPropertyChanged
    {
        private string name, instructions, ingrediencesStr, search;
        public Guid ID { get; set; }
        private List<Ingredience> ingrediences { get; set; }
        public string IngrediencesStr
        {
            get
            {
                return ingrediencesStr;
            }
            set
            {
                ingrediencesStr = value;
            }
        }
        public string Search
        {
            get
            {
                return search;
            }
            set
            {
                search = value;
                OnPropertyChanged("Search");
            }
        }

        [JsonIgnore]
        public static List<Recipe> AllRecipes { get; set; } = new List<Recipe>();

        [JsonIgnore]
        public static List<Recipe> RecipesToShow { get; set; } = new List<Recipe>();
        [JsonIgnore]
        public List<Ingredience> Ingrediences
        {
            get
            {
                return ingrediences;
            }
            set
            {
                ingrediences = value;
                OnPropertyChanged("Ingrediences");
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Instructions
        {
            get
            {
                return instructions;
            }
            set
            {
                instructions = value;
                OnPropertyChanged("Instructions");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Recipe()
        {
            ID = Guid.NewGuid();
            Ingrediences = new List<Ingredience>();
        }
        public Recipe(string rName, string instructs, Guid iD, List<Ingredience> ingrediences)
        {
            Name = rName;
            Instructions = instructs;
            ID = iD;
            Ingrediences = ingrediences;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < ingrediences.Count; i++)
            {
                if (i == ingrediences.Count - 1)
                {
                    sb.Append(Ingredience.SerializeIngredience(ingrediences[i]));
                }
                else
                {
                    sb.Append($"{Ingredience.SerializeIngredience(ingrediences[i])} ");
                }
            }
            IngrediencesStr = sb.ToString();
        }
        public static Recipe CopyRecipe(Recipe recipe)
        {
            return new Recipe(recipe.Name, recipe.Instructions, recipe.ID, recipe.Ingrediences);
        }

        public bool IsOK()
        {
            bool nameOK = Name != string.Empty;
            bool instructionsOK = Instructions != string.Empty;
            bool ingrediencesOK = false;
            foreach (var item in Ingrediences)
            {
                ingrediencesOK = item.IName != string.Empty && item.RegexAmount.IsMatch(item.Amount);
                if (!ingrediencesOK)
                {
                    break;
                }
            }
            return nameOK && instructionsOK && ingrediencesOK;
        }


        public void ClearRecipe()
        {
            Name = string.Empty;
            Instructions = string.Empty;
            ID = Guid.NewGuid();
            Ingrediences = new List<Ingredience>();
            IngrediencesStr = string.Empty;
        }

        private void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
        public static List<Ingredience> StrIngredsToListIngreds(string str)
        {
            string[] ingreds = str.Split(' ');
            List<Ingredience> returnList = new List<Ingredience>();
            foreach (var item in ingreds)
            {
                returnList.Add(Ingredience.DeserializeIngredience(item));
            }
            return returnList;
        }
        public static string SerializeRecipe(Recipe recipe)
        {
            return JsonSerializer.Serialize(recipe);
        }
        public static Recipe DeserializeRecipe(string recipe)
        {
            return JsonSerializer.Deserialize<Recipe>(recipe);
        }
        public bool IsEmpty()
        {
            return Name != string.Empty && Name != null && instructions != string.Empty && instructions != null && ingrediencesStr != string.Empty & ingrediencesStr != null;
        }
        public static void UpdateToMatchFilter(Recipe recipe)
        {
            RecipesToShow.Clear();
            if (recipe.Search == null || recipe.Search == string.Empty)
            {
                foreach (var item in AllRecipes)
                {
                    RecipesToShow.Add(item);
                }
            }
            else
            {
                foreach (var item in AllRecipes)
                {
                    if (item.Name.Contains(recipe.Search.ToLower()))
                    {
                        RecipesToShow.Add(item);
                    }
                }
            }
        }
    }

    class Ingredience
    {
        private string iName;
        public Guid ID { get; set; }

        public string IName
        {
            get
            {
                return iName;
            }
            set
            {
                iName = value;
            }
        }
        public Regex RegexAmount = new Regex("^(\\d{1,4})([a-zA-Z]{1,4})$");
        private string amount;
        public string Amount
        {
            get
            {
                return amount;
            }
            set
            {
                amount = value;
            }
        }
        public Ingredience()
        {
            ID = Guid.NewGuid();
            IName = string.Empty;
            Amount = string.Empty;
        }
        public static string SerializeIngredience(Ingredience ingredience)
        {
            try
            {
                return JsonSerializer.Serialize(ingredience);
            }
            catch (Exception)
            {
                return string.Empty;
            }

        }
        public static Ingredience DeserializeIngredience(string ingredience)
        {
            try
            {
                return JsonSerializer.Deserialize<Ingredience>(ingredience);
            }
            catch (Exception)
            {
                return new Ingredience();
            }
        }
        public static List<Ingredience> DeserializeInstructsStr(string ingrediences)
        {
            string[] ings = ingrediences.Split(' ');
            List<Ingredience> retIngreds = new List<Ingredience>();
            foreach (var item in ings)
            {
                retIngreds.Add(DeserializeIngredience(item));
            }
            return retIngreds;
        }
    }
}