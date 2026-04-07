import "../styles/ProductCard.css"
import {Link} from "react-router-dom";

//@ts-ignore
export const ProductCard = ({product}) => {
    
    return (
        <Link to={`/products/${product.barcode}`}>
            <div className="productCard">
                <img src={product.imageUrl}
                     width={200}
                     height={200}
                     alt="Epic album cover"/>
                <p> {product.artist} - {product.name}</p>
            </div>
        </Link>
    )
}