import "../styles/AlbumCard.css"
import PopOutCard from "./PopOutCard.tsx";
import {useEffect, useRef, useState} from "react";
import Product from "../models/Product.ts";
import authClient from "../api/AdminClient.ts";

//@ts-ignore
export const AlbumCard = ({product, best, rerender}) => {
    const [isOpen, setIsOpen] = useState(false);
    const inputRef = useRef<HTMLParagraphElement>(null);
    
    // Needs to be at the first level of the component
    // rerender is flipping every time
    // so useEffect is called on every fetch
    // used to put focus on the price input field on the best match
    useEffect(() => {
        if(best) {
            console.log(best);
            inputRef.current?.focus();
        }
    }, [inputRef, best, rerender]);
    
    const acceptProduct = async () => {
        try {
            const res = await authClient.post("/Product/AddProduct", {
                product
            })
        } catch (e) {
            return;
        }
    }
    
    return (
        <>
            <div className={"albumCard " + (best && "bestCard")} onKeyUp={(e) => {
                if (e.key === 'Escape' && isOpen)
                    setIsOpen(false)
            }}>
                {/* div za pozadinsku sliku */}
                <div className="background-style" style={{
                    background: "url(" + `${product.imageUrl}` + ") center",
                    zIndex: "-1"
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
                             style={{pointerEvents: "none"}}/>
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
                                        selected={item === product.type}>{Product.evaluateType(item)}</option>
                                })
                            }
                        </select>
                    </div>
                    <div className="productInput textBord">
                        <p>Price:</p>
                        <div contentEditable="plaintext-only" className="iField" spellCheck="false"
                             onInput={(r) => {
                                 product.price = r.currentTarget.textContent;
                             }} ref={inputRef}>
                            <p>{product.price}</p>
                        </div>
                    </div>
                    <div>
                        <button className="acceptButton" onClick={acceptProduct}>Add Product</button>
                        <button className="acceptButton" id="details" onClick={() => setIsOpen(true)}>Details</button>
                    </div>
                </div>
            </div>
            <div className="pop-out" onKeyUp={(e) => {
                if (e.key === 'Escape' && isOpen)
                    setIsOpen(false);
            }}>
                <PopOutCard
                    isOpen={isOpen}
                    onClose={() => setIsOpen(false)}
                    title={product.name}
                    backgroundImage={product.imageUrl}
                >
                    <img src={product.imageUrl} style={{width: "100%"}}/>
                    <p style={{textAlign: "center"}}><strong>Tracklist:</strong></p>
                    {product.tracklist.map((_: any, i: number) => {
                        return (
                            <p contentEditable="plaintext-only" className="iField" spellCheck="false"
                               onInput={(t) => {
                                   product.tracklist[i] = t.currentTarget.textContent;
                                   console.log(t.currentTarget.textContent);
                               }}>{product.tracklist[i]}</p>)
                    })}
                    <p style={{textAlign: "center"}}><strong>Runtime:</strong></p>
                    {
                        <p contentEditable="plaintext-only" className="iField" spellCheck="false"
                           onInput={(r) => {
                               product.runtime = r.currentTarget.textContent;
                           }}>{product.runtime}</p>
                    }

                </PopOutCard>
            </div>
        </>

    );

}