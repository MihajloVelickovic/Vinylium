import "../styles/AlbumCard.css"
import axios from "axios";
import PopOutCard from "./PopOutCard.tsx";
import {useState} from "react";

export const AlbumCard = ({product}) => {
    const [isOpen, setIsOpen] = useState(false);
    const acceptProduct = async () => {
        try {
            const res = await axios.post("http://localhost:1738/api/Product/AddProduct", {
                product
            })
            if (res.status === 200) {
                console.log("PRODUCT:" + res.data)
            }
        } catch (e) {
            console.log("Exception: " + e);
            return;
        }
    }

    return (
        <>
            <div className="albumCard">
                {/* div za pozadinsku sliku */}
                <div className="background-style" style={{
                    background: "url(" + `${product.imageUrl}` + ") center",
                }}>
                </div>

                {/* div za podatke i sliku */}
                <div style={{
                    backdropFilter: "blur(0px)"
                }}>
                    <div className="image">
                        <img src={product.imageUrl}
                             width={100}
                             height={100}
                             style={{borderRadius: "5%", pointerEvents: "none"}}/>
                    </div>
                    <div className="productInput textBord">
                        <p>Barcode:</p>
                        <div contentEditable="plaintext-only" className="iField" spellCheck="false"
                             onInput={(b) => {
                                 product.barcode = b.currentTarget.textContent;
                             }}>
                            <p>{product.barcode}</p>
                        </div>
                    </div>

                    <div className="productInput textBord">
                        <p>CatNo:</p>
                        <div contentEditable="plaintext-only" className="iField" spellCheck="false"
                             onInput={(c) => {
                                 product.catalogNumber = c.currentTarget.textContent;
                             }}>
                            <p>{product.catalogNumber}</p>
                        </div>
                    </div>

                    <div className="productInput textBord">
                        <p>Title:</p>
                        <div contentEditable="plaintext-only" className="iField" spellCheck="false"
                             onInput={(n) => {
                                 product.name = n.currentTarget.textContent;
                             }}>
                            <p>{product.name}</p>
                        </div>
                    </div>

                    <div className="productInput textBord">
                        <p>Artist:</p>
                        <div contentEditable="plaintext-only" className="iField" spellCheck="false"
                             onInput={(a) => {
                                 product.artist = a.currentTarget.textContent;
                             }}>
                            <p>{product.artist}</p>
                        </div>
                    </div>
                    <div className="productInput textBord">
                        <p>Release Date:</p>
                        <div contentEditable="plaintext-only" className="iField" spellCheck="false"
                             onInput={(r) => {
                                 product.releaseDate = r.currentTarget.textContent;
                             }}>
                            <p>{product.releaseDate}</p>
                        </div>
                    </div>
                    <div className="productInput textBord">
                        <p>Type:</p>
                        <select onChange={(t) => {
                            product.type = t.target.selectedIndex;
                        }}>
                            {
                                [0, 1, 2].map((item) => {
                                    return <option
                                        selected={item === product.type}>{product.evaluateType(item)}</option>
                                })
                            }
                        </select>
                    </div>
                    <div>
                        <button className="acceptButton" onClick={acceptProduct}>Add Product</button>
                        <button className="detailsButton" onClick={() => setIsOpen(true)}>Vidi više</button>
                    </div>
                </div>
            </div>
            <div className="pop-out">
                <PopOutCard
                    isOpen={isOpen}
                    onClose={() => setIsOpen(false)}
                    title={product.name}
                    backgroundImage={product.imageUrl}
                >
                    <img src={product.imageUrl} style={{ width: "100%", borderRadius: "8px" }}/>
                    <p><strong>Artist:</strong> {product.artist}</p>
                    <p><strong>Barcode:</strong> {product.barcode}</p>
                    <p><strong>CatNo:</strong> {product.catalogNumber}</p>
                    <p><strong>Release Date:</strong> {product.releaseDate}</p>
                    <p><strong>Type:</strong> {product.evaluateType(product.type)}</p>
                    
                </PopOutCard>
            </div>
        </>

    );

}