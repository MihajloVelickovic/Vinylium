import Product from "../models/Product";
import "../styles/ProductCard.css"
import {Link} from "react-router-dom";
import {useState} from "react";

//@ts-ignore
export const ProductCard = ({product}) => {
    
    const [selected, setSelected] = useState(false);
    
    return (
        <Link to={`/products/${product.barcode}`} style={{textDecoration: "none", color: "var(--text)"}}>
            <div className="productCard" onMouseEnter={()=>setSelected(true)} onMouseLeave={()=>setSelected(false)}>
                <div>
                <img src={product.imageUrl}
                     width={200}
                     height={200}
                     alt="Epic album cover"/>
                    {/* style={ selected ? {scale: "1.1"} : {scale: "1.0"}}/>*/}
                </div>
                <div>
                    <p>{product.artist} - {product.name} ({Product.evaluateType(product.type)})</p>
                    <p>{product.price} RSD</p>
                </div>
            </div>
        </Link>
    )
}