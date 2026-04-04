import "../styles/AlbumCard.css"
import axios from "axios";


export const AlbumCard = ({product}) => {

    const acceptProduct = async () => {
        console.log("kliknut sam");
        try {
           const res = await axios.post("http://localhost:1738/api/Product/AddProduct", {
                product
            })
            if (res.status===200){
                console.log(res.data)
            }
        }
        catch (e) {
            console.log("Exception: " + e);
            return;
        }
        
    }
    
    return (
        <div className = "albumCard">
            {/* div za pozadinsku sliku */}
            <div className="background-style" style={{
                background: "url("+`${product.imageUrl}`+") center",
            }}>
            </div>

            {/* div za podatke i sliku */}
            <div style={{
                backdropFilter: "blur(0px)"
            }}>
                <img src={product.imageUrl} 
                    width={100} 
                    height={100} 
                    style={{borderRadius: "5%"}}/>

                <div className="productInput textBord">
                    <p>Barcode:</p>
                    <div contentEditable="true" className="iField">
                        <p>{product.barcode}</p>
                    </div>
                </div>

                <div className="productInput textBord">
                    <p>CatNo:</p>
                    <div contentEditable="true" className="iField">
                        <p>{product.catalogNumber}</p>
                    </div>
                </div>

                <div className="productInput textBord">
                    <p>Title:</p>
                    <div contentEditable="true" className="iField">
                        <p>{product.name}</p>
                    </div>
                </div>

                <div className="productInput textBord">
                    <p>Artist:</p>
                    <div contentEditable="true" className="iField">
                        <p>{product.artist}</p>
                    </div>
                </div>
                <div className="productInput textBord">
                    <p>Release Date:</p>
                    <div contentEditable="true" className="iField">
                        <p>{product.releaseDate}</p>
                    </div>
                </div>
                <div className="productInput textBord">
                    <p>Type:</p>
                    <div contentEditable="true" className="iField">
                        <p>{product.type}</p>
                    </div>
                </div>
                <div >
                    <button className="acceptButton" onClick={acceptProduct}>Add Product</button>
                </div>

            </div>
        </div>
    );

}