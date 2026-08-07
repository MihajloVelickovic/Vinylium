import "../styles/AlbumCard.css"
import PopOutCard from "./PopOutCard.tsx";
import {useEffect, useRef, useState} from "react";
import Product from "../models/Product.ts";
import authClient from "../api/AuthClient";
import Store from "../models/Store.ts";
import store from "./Store.tsx";
import StoreQuantityPair from "../models/StoreQuantityPair.ts";

// .focus() on a contentEditable does not move the caret, so it can land in
// front of an existing value and the next keystroke ends up in the wrong place
const focusEnd = (el: HTMLElement) => {
    el.focus();
    const range = document.createRange();
    range.selectNodeContents(el);
    range.collapse(false);
    const selection = window.getSelection();
    selection?.removeAllRanges();
    selection?.addRange(range);
}
//@ts-ignore
export const AlbumCard = ({product, best}) => {
    const [isOpen, setIsOpen] = useState(false);
    const priceRef = useRef<HTMLDivElement>(null);
    // the price is the one field Discogs cannot fill in, so it gets called out
    // until something is typed into it
    const [showPriceHint, setShowPriceHint] = useState(true);
    const [storeQuantities, setStoreQuantities] = useState<Array<StoreQuantityPair>>([])
    const [loading, setLoading] = useState(true);
    // runs once per mount, and FetchAlbumsForm remounts this card on every
    // fetch and every newly picked match by changing its key
    useEffect(() => {

        const getStores = async () => {
            const pairs = new Array<StoreQuantityPair>();
            try {
                const r = await authClient.get("/Store/GetStores")
                r.data.data.forEach((store: any) => {
                    const item = new StoreQuantityPair(new Store(store), "0");
                    pairs.push(item);
                })
            }
            catch(e){
                console.error(e);
            }
            return pairs;
        }
        
        if(priceRef.current)
            focusEnd(priceRef.current);

        getStores().then(p => {
            console.log(p);
            setStoreQuantities(p);
            setLoading(false)
        }).catch(e => console.error(e));
        
    }, []);
    
    
    
    const acceptProduct = async () => {
        try {
            await authClient.post("/Product/AddProduct", {
                product,
                storeQuantities
            })
        } catch (e) {
            return;
        }
    }
    
    return (
        <>
            <div className={"albumCard" + (best ? " bestCard" : "")} onKeyUp={(e) => {
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
                <div className="cardBody">
                    <div className="image">
                        <img src={product.imageUrl}
                             width={220}
                             height={220}
                             style={{pointerEvents: "none"}}/>
                    </div>

                    <div className="cardFields">
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
                        <div >
                            <p>Availability:</p>
                        {
                            (!loading && storeQuantities !== null) &&
                            storeQuantities.map((s:StoreQuantityPair) => {
                                return (
                                    <div key={s.store.name} className="productInput textBord">
                                        <p>{s.store.name}</p>
                                        <div contentEditable="plaintext-only" className="iField" spellCheck="false"
                                        onInput={(t) => {
                                            const newQuant = t.currentTarget.textContent ?? "";
                                            setStoreQuantities(prev =>
                                                prev.map(ss =>
                                                    ss.store.name === s.store.name ? 
                                                    new StoreQuantityPair(new Store(s.store), newQuant) : 
                                                    ss
                                                )
                                            );
                                            console.log(s.quantity);
                                            console.log(s.store.contactNumber);
                                        }}>
                                            <p>{s.quantity}</p>
                                        </div>
                                    </div>
                            )
                            })
                        }
                        </div>
                        <div className="productInput textBord priceRow">
                            <p>Price:</p>
                            <div contentEditable="plaintext-only" className="iField" spellCheck="false"
                                 onInput={(r) => {
                                     product.price = r.currentTarget.textContent;
                                     setShowPriceHint(false);
                                 }} ref={priceRef}>
                                <p>{product.price}</p>
                            </div>
                            {showPriceHint &&
                                <div className="priceHint" role="tooltip"
                                     onClick={() => setShowPriceHint(false)}>
                                    Set a price before adding
                                </div>
                            }
                        </div>
                    </div>
                </div>

                <div className="cardActions">
                    <button className="acceptButton" onClick={acceptProduct}>Add Product</button>
                    <button className="acceptButton detailsButton" onClick={() => setIsOpen(true)}>Details</button>
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