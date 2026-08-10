import "../styles/AlbumCard.css"
import PopOutCard from "./PopOutCard.tsx";
import {useEffect, useRef, useState} from "react";
import Product from "../models/Product.ts";
import authClient from "../api/AuthClient";
import Store from "../models/Store.ts";
import StoreQuantityPair from "../models/StoreQuantityPair.ts";
import {Field} from "./Field.tsx";
import {useProductDraft} from "../hooks/useProductDraft.ts";
import Track from "../models/Track.ts";

export const AlbumCard = ({product, best}: { product: Product, best: boolean }) => {
    const [isOpen, setIsOpen] = useState(false);
    const priceRef = useRef<HTMLInputElement>(null);
    // the price is the one field Discogs cannot fill in, so it gets called out
    // until something is typed into it
    const [showPriceHint, setShowPriceHint] = useState(true);
    const [storeQuantities, setStoreQuantities] = useState<Array<StoreQuantityPair>>([])
    const [loading, setLoading] = useState(true);
    // edits live here and die with the card, which is what we want: picking a
    // different match remounts this component and should start clean
    const {draft, setField, toPayload} = useProductDraft(product);

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
            } catch (e) {
                console.error(e);
            }
            return pairs;
        }

        priceRef.current?.focus();

        getStores().then(p => {
            setStoreQuantities(p);
            setLoading(false)
        }).catch(e => console.error(e));

    }, []);

    // reuses the store instance instead of rebuilding it: Store's constructor
    // reads openingTime/closingTime off raw json, so feeding it an existing
    // Store (which has openingHours/closingHours) blanks both
    const setQuantity = (storeId: number, quantity: string) => {
        setStoreQuantities(prev => prev.map(p =>
            p.store.id === storeId ? new StoreQuantityPair(p.store, quantity) : p
        ));
    }

    const acceptProduct = async () => {
        try {
            await authClient.post("/Product/AddProduct", {
                product: toPayload(),
                // Quantity is an int server side, so a box the user cleared has
                // to go out as "0" rather than ""
                //TODO
                storeQuantities: storeQuantities.map(p =>
                    new StoreQuantityPair(p.store, p.quantity.trim() || "0"))
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
                    background: "url(" + `${draft.imageUrl}` + ") center",
                    zIndex: "-1"
                }}>
                </div>

                {/* div za podatke i sliku */}
                <div className="cardBody">
                    <div className="image">
                        <img src={draft.imageUrl}
                             width={220}
                             height={220}
                             style={{pointerEvents: "none"}}/>
                    </div>

                    <div className="cardFields">
                        <Field label="Barcode:" value={draft.barcode}
                               onChange={v => setField("barcode", v)}/>

                        <Field label="CatNo:" value={draft.catalogNumber}
                               onChange={v => setField("catalogNumber", v)}/>

                        <Field label="Title:" value={draft.name}
                               onChange={v => setField("name", v)}/>

                        <Field label="Artist:" value={draft.artist}
                               onChange={v => setField("artist", v)}/>

                        <Field label="Release Date:" value={draft.releaseDate}
                               onChange={v => setField("releaseDate", v)}/>

                        <div className="productInput textBord">
                            <p>Type:</p>
                            <select value={draft.type}
                                    onChange={(t) => setField("type", Number(t.target.value))}>
                                {
                                    [0, 1, 2].map((item) => {
                                        return <option key={item}
                                                       value={item}>{Product.evaluateType(item)}</option>
                                    })
                                }
                            </select>
                        </div>

                        <div>
                            <p>Availability:</p>
                            {
                                !loading &&
                                storeQuantities.map((s: StoreQuantityPair) => {
                                    return (
                                        <Field key={s.store.id}
                                               label={s.store.name}
                                               value={s.quantity}
                                               inputMode="numeric"
                                               onChange={v => setQuantity(s.store.id, v)}/>
                                    )
                                })
                            }
                        </div>

                        <Field label="Price:" value={draft.price}
                               rowClassName="priceRow"
                               inputRef={priceRef}
                               inputMode="decimal"
                               placeholder="0.00"
                               onChange={v => {
                                   setField("price", v);
                                   setShowPriceHint(false);
                               }}>
                            {showPriceHint &&
                                <div className="priceHint" role="tooltip"
                                     onClick={() => setShowPriceHint(false)}>
                                    Set a price before adding
                                </div>
                            }
                        </Field>
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
                    title={draft.name}
                    backgroundImage={draft.imageUrl}
                >
                    <img src={draft.imageUrl} style={{width: "100%"}}/>
                    <p style={{textAlign: "center"}}><strong>Tracklist:</strong></p>
                    {/* keying on the index is safe here specifically because the
                        tracklist is never reordered, filtered or appended to */}
                    {draft.tracklist.map((track: Track, i: number) => {
                        return (
                            <div className="albumCardPopupRow">
                                <input key={i} className="iField" type="text" spellCheck={false}
                                     value={track.title}
                                     onChange={(t) => setField("tracklist",
                                         draft.tracklist.map((x: Track, j) => {
                                             return j === i ? new Track({title: t.target.value, runtime: x.runtime}) : x;
                                         }))}/>
                                <input key={i}
                                       className="iField"
                                       type="text"
                                       spellCheck={false}
                                       value={track.runtime}
                                       onChange={(t) => setField("tracklist",
                                           draft.tracklist.map((x: Track, j) => {
                                               return j === i ? new Track({title: x.title, runtime: t.target.value}) : x;
                                           }))}/>
                            </div>)
                    })}
                </PopOutCard>
            </div>
        </>

    );

}
