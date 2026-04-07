import "../styles/ProductCard.css"
import {Link} from "react-router-dom";

//@ts-ignore
export const ProductCard = ({product}) => {
    
    return (
        <Link to={`/products/${product.barcode}`} style={{textDecoration: "none", color: "var(--text)"}}>
            <div className="productCard">
                <div>
                <img src={product.imageUrl}
                     width={200}
                     height={200}
                     alt="Epic album cover"/>
                </div>
                <div>
                    <p>{product.artist} - {product.name} ({product.evaluateType(product.type)})</p>
                    <p>{product.price} RSD</p>
                </div>
            </div>
        </Link>
    )
}