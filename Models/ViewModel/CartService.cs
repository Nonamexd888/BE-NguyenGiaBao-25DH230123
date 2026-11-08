using System.Web;

namespace BE_NguyenGiaBao_25DH230123.Models.ViewModel
{
    public class CartService
    {
        private readonly HttpSessionStateBase session;

        public CartService(HttpSessionStateBase session)
        {
            this.session = session;
        }

        public Cart GetCart()
        {
            var cart = (Cart)session["Cart"];
            if (cart == null)
            {
                cart = new Cart();
                session["Cart"] = cart;
            }
            return cart;
        }

        public void ClearCart()
        {
            session["Cart"] = null;
        }
    }
}