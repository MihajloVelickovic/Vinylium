import Product from "../models/Product.ts";
import "../styles/ManageProductCard.css"
export const ManageProductCard = ({product}:{product: Product}) => {
    return (
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
    )/*name: string;
    artist: string;
    imageUrl: string;
    releaseDate: string;
    type: number;
    barcode: string;
    catalogNumber: string;
    inwarehouse: boolean;
    runtime: string;
    price: string | null;
    tracklist: Array<string>;*/
}