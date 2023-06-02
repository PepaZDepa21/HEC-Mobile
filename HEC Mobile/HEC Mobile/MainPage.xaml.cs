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
using Xamarin.Essentials;
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
            Recipe.UpdateToMatchFilter(SearchEntry.Text);
            UpdateRecipeListview();
        }

        public void UpdateRecipeListview()
        {
            RecipesList.ItemsSource = null;
            RecipesList.ItemsSource = Recipe.RecipesToShow;
            WriteRecipesToFile();
            Recipe.UpdateToMatchFilter(SearchEntry.Text);
        }

        public static void WriteRecipesToFile()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Recipes.txt")))
                {
                    foreach (var item in Recipe.AllRecipes)
                    {
                        sw.WriteLine(Recipe.SerializeRecipe(item));
                    }
                    sw.Flush();
                }
            }
            catch (Exception) { }
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
            Navigation.PushAsync(new ReadRecipe(Recipe.AllRecipes.IndexOf((Recipe)button.BindingContext)));
            UpdateRecipeListview();
        }
        private void BtnEditClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Navigation.PushAsync(new EditRecipe(Recipe.AllRecipes.IndexOf((Recipe)button.BindingContext)));
            UpdateRecipeListview();
        }
        private async void BtnDeleteClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            Recipe recipe = (Recipe)button.BindingContext;
            if (await DisplayAlert($"Delete {recipe.Name}", $"Do you want to delete {recipe.Name}?", "Yes", "No"))
            {
                Recipe.AllRecipes.Remove(recipe);
                Recipe.UpdateToMatchFilter(SearchEntry.Text);
                UpdateRecipeListview();
            }
        }
        private void BtnNewRecipeClicked(object sender, EventArgs e)
        {
            Recipe.AllRecipes.Add(new Recipe());
            Navigation.PushAsync(new EditRecipe(Recipe.AllRecipes.Count - 1));
            Recipe.UpdateToMatchFilter(SearchEntry.Text);
        }
        private async void BtnImportClicked(object sender, EventArgs e)
        {
            try
            {
                var readStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

                if (readStatus != PermissionStatus.Granted)
                {
                    readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
                }

                //if (readStatus != PermissionStatus.Granted)
                //{
                //    await DisplayAlert("Permission not granted", "Permission to read files not granted", "Ok");
                //    return;
                //}
                var file = await FilePicker.PickAsync();
                if (file != null)
                {
                    string path = file.FullPath;
                    File.ReadAllText(path);
                }
            }
            catch (Exception)
            {

            }
        }
        private void BtnRandomClicked(object sender, EventArgs e)
        {
            int index = new Random().Next(Recipe.AllRecipes.Count);
            Navigation.PushAsync(new ReadRecipe(index));
        }
        private void BtnSearchClicked(object sender, EventArgs e)
        {
            Recipe.UpdateToMatchFilter(SearchEntry.Text);
            UpdateRecipeListview();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Recipe.UpdateToMatchFilter(SearchEntry.Text);
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
                ingrediencesOK = item.RegexAmount.IsMatch(item.Amount);
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
        public static void ListIngredsToStrIngreds(Recipe recipe)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < recipe.ingrediences.Count; i++)
            {
                if (i == recipe.ingrediences.Count - 1)
                {
                    sb.Append(Ingredience.SerializeIngredience(recipe.ingrediences[i]));
                }
                else
                {
                    sb.Append($"{Ingredience.SerializeIngredience(recipe.ingrediences[i])} ");
                }
            }
            recipe.IngrediencesStr = sb.ToString();
        }
        public static string SerializeRecipe(Recipe recipe)
        {
            ListIngredsToStrIngreds(recipe);
            return JsonSerializer.Serialize(recipe);
        }
        public static Recipe DeserializeRecipe(string recipe)
        {
            Recipe r = JsonSerializer.Deserialize<Recipe>(recipe);
            r.Ingrediences = StrIngredsToListIngreds(r.IngrediencesStr);
            return Recipe.CopyRecipe(r);
        }
        public bool IsEmpty()
        {
            return Name != string.Empty && Name != null && instructions != string.Empty && instructions != null && ingrediencesStr != string.Empty & ingrediencesStr != null;
        }
        public static void UpdateToMatchFilter(string filter)
        {
            RecipesToShow.Clear();
            if (filter == null || filter == string.Empty)
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
                    if (item.Name.ToLower().Contains(filter.ToLower()))
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