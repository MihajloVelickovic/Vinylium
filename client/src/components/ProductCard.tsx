import Product from "../models/Product";
import "../styles/ProductCard.css"
import {Link} from "react-router-dom";

//@ts-ignore
export const ProductCard = ({product}: {product: Product}) => {

    return (
        <div className="productCard">
            <Link to={`/products/${product.barcode}`} style={{textDecoration: "none", color: "var(--text)"}}>
                <div>
                    <img src={product.imageUrl}
                         width={200}
                         height={200}
                         alt={product.artist+ ' - ' + product.name}
                         style={!product.inStock ? {filter: "sepia(100%)"} : {}}/>
                </div>
                <div>
                    <p>{product.artist} - {product.name} ({Product.evaluateType(product.type)})</p>
                    <p>{product.price} RSD</p>
                </div>
            </Link>
            
        </div>

    )
}