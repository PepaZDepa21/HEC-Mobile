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
        public EditRecipe(int recipeIndex)
        {
            InitializeComponent();
            index = recipeIndex;
            NavigationPage.SetHasNavigationBar(this, false);
            Recipe recipeToEdit = Recipe.CopyRecipe(Recipe.RecipesToShow[recipeIndex]);
            InstructionsEditor.Text = recipeIndex.ToString();
        }

        private void BtnAddIngredienceClicked(object sender, EventArgs e)
        {

        }
        private void BtnRemoveIngredienceClicked(object sender, EventArgs e)
        {

        }
        private void BtnSaveClicked(object sender, EventArgs e)
        {
            
            Navigation.PopAsync();
        }
        private void BtnUndoClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}