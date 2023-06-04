using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HEC_Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class EditRecipe : ContentPage
    {
        int index = 0;
        Recipe recipeToEdit = new Recipe();
        bool addRecipe = false;
        public EditRecipe(int recipeIndex)
        {
            InitializeComponent();
            index = recipeIndex;
            NavigationPage.SetHasNavigationBar(this, false);
            if (Recipe.AllRecipes.Count == index)
            {
                recipeToEdit = new Recipe();
                addRecipe = true;
            }
            else
            {
                recipeToEdit = Recipe.CopyRecipe(Recipe.AllRecipes[recipeIndex]);

            }
            BindingContext = recipeToEdit;
            IngrediencesList.ItemsSource = recipeToEdit.Ingrediences;
        }

        public void UpdateIngredienceListview()
        {
            IngrediencesList.ItemsSource = null;
            IngrediencesList.ItemsSource = recipeToEdit.Ingrediences;
        }

        private void BtnAddIngredienceClicked(object sender, EventArgs e)
        {
            recipeToEdit.Ingrediences.Add(new Ingredience());
            UpdateIngredienceListview();
        }
        private void BtnRemoveIngredienceClicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            recipeToEdit.Ingrediences.Remove((Ingredience)button.BindingContext);
            UpdateIngredienceListview();
        }
        private async void BtnSaveClicked(object sender, EventArgs e)
        {
            if (recipeToEdit.IsOK())
            {
                if (addRecipe)
                {
                    Recipe.AllRecipes.Add(Recipe.CopyRecipe(recipeToEdit));
                }
                else
                {
                    Recipe.AllRecipes[index] = Recipe.CopyRecipe(recipeToEdit);
                }
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Cannot save recipe", "Some values are in appropriate", "Ok");
            }
        }
        private void BtnUndoClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}