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
    public partial class ReadRecipe : ContentPage
    {
        public ReadRecipe(int recipeIndex)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            Recipe recipeToEdit = Recipe.CopyRecipe(Recipe.RecipesToShow[recipeIndex]);
            
        }

        private void BtnBackClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}