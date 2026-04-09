import {Link} from "react-router-dom";


function Cart() {
    return (
        <Link className="nav-link" to="/cart" id="cart">
            <img
                className="cart-img"
                src="../src/assets/cart.svg"
            />
        </Link>
    );
}
export default Cart;