import Product from "../models/Product.ts";
import "../styles/ManageProductCard.css"
import {Link} from "react-router-dom";

export const ManageProductCard = ({product}:{product: Product}) => {
    return (
        <Link to={`/admin/manage-products/${product.barcode}`} style={{textDecoration: "none", color: "var(--text)"}}>
            <div className="manage-product-card">
                <div>
                    <img src={product.imageUrl}
                         width={200}
                         height={200}
                         alt={product.artist+ ' - ' + product.name}/>
                </div>
                <div>
                    <div className="infoBox">
                        <p>Barcode:</p>
                        <p>{product.barcode}</p>
                    </div>
                    <div className="infoBox">
                        <p>Catalog Number:</p>
                        <p>{product.catalogNumber}</p>
                    </div>
                    <div className="infoBox">
                        <p>Name:</p>
                        <p>{product.name}</p>
                    </div>
                    <div className="infoBox">
                        <p>Artist:</p>
                        <p>{product.artist}</p>
                    </div>
                    <div className="infoBox">
                        <p>Release Date:</p>
                        <p>{product.releaseDate}</p>
                    </div>
                    <div className="infoBox">
                        <p>Type:</p>
                        <p>{Product.evaluateType(product.type)}</p>
                    </div>
                    <div className="infoBox">
                        <p>Price:</p>
                        <p>{product.price}</p>
                    </div>
                </div>
            </div>
        </Link>
            
    )
}