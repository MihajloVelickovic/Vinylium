import "../styles/AlbumCard.css";
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

    const [showPriceHint, setShowPriceHint] = useState(true);
    const [storeQuantities, setStoreQuantities] = useState<Array<StoreQuantityPair>>([]);
    const [loading, setLoading] = useState(true);

    const {draft, setField, toPayload} = useProductDraft(product);

    useEffect(() => {
        const getStores = async () => {
            const pairs = new Array<StoreQuantityPair>();

            try {
                const r = await authClient.get("/Store/GetStores");

                r.data.data.forEach((store: any) => {
                    const item = new StoreQuantityPair(new Store(store), 0);
                    pairs.push(item);
                });
            } catch (e) {
                console.error(e);
            }

            return pairs;
        };

        priceRef.current?.focus();

        getStores()
            .then(p => {
                setStoreQuantities(p);
                setLoading(false);
            })
            .catch(e => console.error(e));
    }, []);

    const setQuantity = (storeId: number, quantity: string) => {
        setStoreQuantities(prev =>
            prev.map(p => {
                return p.store.id === storeId ?
                       new StoreQuantityPair(p.store, parseInt(quantity)) :
                       p;
                }
            )
        );
    };

    const calculateRuntime = (list: Array<Track>): string => {
        const totalMinutes = list.reduce((total, track) => {
            
            const splitTime = track.runtime.split(":").map(Number);

            const hasHours = splitTime.length === 3;
            const hours = hasHours ? splitTime[0] : 0;
            const minutes = hasHours ? splitTime[1] : splitTime[0];
            const seconds = hasHours ? splitTime[2] : splitTime[1];

            return total + hours * 60 + minutes + seconds / 60;
        }, 0);

        const hours = Math.floor(totalMinutes / 60);
        const minutes = Math.floor(totalMinutes % 60);
        const seconds = Math.floor((totalMinutes % 1) * 60);
        
        return (hours > 0 ? `${String(hours).padStart(2, "0")}:` : ``) +
               `${String(minutes).padStart(2, "0")}:` +
               `${String(seconds).padStart(2, "0")}`;
        
        
    };

    const updateTrack = (index: number, field: keyof Track, value: string) => {
        const tracklist = draft.tracklist.map((track, i) => {
                return i === index ? 
                       {...track, [field]: value} : 
                       track;
            }
        );
        setField("tracklist", tracklist);
        setField("runtime", calculateRuntime(tracklist));
        
    };

    const handleRemoveSong = (index: number) => {
        const tracklist = draft.tracklist.filter((_, i) => i !== index);
        setField("tracklist", tracklist);
        setField("runtime", calculateRuntime(tracklist));
    };

    const acceptProduct = async () => {
        try {
            await authClient.post("/Product/AddProduct", {
                product: toPayload(),
                storeQuantities
            });
        } catch (e) {
            return;
        }
    };

    return (
        <>
            <div
                className={"albumCard" + (best ? " bestCard" : "")}
                onKeyUp={(e) => {
                    if (e.key === "Escape" && isOpen)
                        setIsOpen(false);
                }}>
                <div
                    className="background-style"
                    style={{
                        background: `url(${draft.imageUrl}) center`,
                        zIndex: "-1"
                    }}/>

                <div className="cardBody">
                    <div className="image">
                        <img
                            src={draft.imageUrl}
                            width={220}
                            height={220}
                            style={{pointerEvents: "none"}}/>
                    </div>

                    <div className="cardFields">
                        <Field
                            label="Barcode:"
                            value={draft.barcode}
                            onChange={v => setField("barcode", v)}/>

                        <Field
                            label="CatNo:"
                            value={draft.catalogNumber}
                            onChange={v => setField("catalogNumber", v)}/>

                        <Field
                            label="Title:"
                            value={draft.name}
                            onChange={v => setField("name", v)}/>

                        <Field
                            label="Artist:"
                            value={draft.artist}
                            onChange={v => setField("artist", v)}/>

                        <Field
                            label="Release Date:"
                            value={draft.releaseDate}
                            onChange={v => setField("releaseDate", v)}/>

                        <div className="productInput textBord">
                            <p>Type:</p>

                            <select
                                value={draft.type}
                                onChange={e =>
                                    setField("type", Number(e.target.value))
                                }>
                                {[0, 1, 2].map(item => (
                                    <option key={item} value={item}>
                                        {Product.evaluateType(item)}
                                    </option>
                                ))}
                            </select>
                        </div>

                        <div>
                            <p>Availability:</p>

                            {!loading &&
                                storeQuantities.map((s: StoreQuantityPair) => (
                                    <Field
                                        key={s.store.id}
                                        label={s.store.name}
                                        value={s.quantity.toString()}
                                        inputMode="numeric"
                                        onChange={(v) =>
                                            setQuantity(s.store.id, v)
                                        }/>
                                ))}
                        </div>

                        <Field
                            label="Price:"
                            value={draft.price}
                            rowClassName="priceRow"
                            inputRef={priceRef}
                            inputMode="decimal"
                            placeholder="0.00"
                            onChange={v => {
                                setField("price", v);
                                setShowPriceHint(false);
                            }}>
                            {showPriceHint && (
                                <div
                                    className="priceHint"
                                    role="tooltip"
                                    onClick={() => setShowPriceHint(false)}>
                                    Set a price before adding
                                </div>
                            )}
                        </Field>
                    </div>
                </div>

                <div className="cardActions">
                    <button
                        className="acceptButton"
                        onClick={acceptProduct}>
                        Add Product
                    </button>

                    <button
                        className="acceptButton detailsButton"
                        onClick={() => setIsOpen(true)}>
                        Details
                    </button>
                </div>
            </div>

            <div
                className="pop-out"
                onKeyUp={(e) => {
                    if (e.key === "Escape" && isOpen)
                        setIsOpen(false);
                }}>
                <PopOutCard
                    isOpen={isOpen}
                    onClose={() => setIsOpen(false)}
                    title={draft.name}
                    backgroundImage={draft.imageUrl}>
                    <img
                        src={draft.imageUrl}
                        style={{width: "100%"}}
                    />

                    <p style={{textAlign: "center"}}>
                        <strong>Tracklist:</strong>
                    </p>

                    {draft.tracklist.map((track: Track, i: number) => (
                        <div
                            className="albumCardPopupRow"
                            key={i}>
                            <input
                                className="iField"
                                type="text"
                                spellCheck={false}
                                value={track.title}
                                onChange={e =>
                                    updateTrack(
                                        i,
                                        "title",
                                        e.target.value
                                    )
                                }/>

                            <input
                                className="iField"
                                type="text"
                                spellCheck={false}
                                value={track.runtime}
                                onChange={e =>
                                    updateTrack(
                                        i,
                                        "runtime",
                                        e.target.value
                                    )
                                }/>

                            <button onClick={() => handleRemoveSong(i)}>
                                X
                            </button>
                        </div>
                    ))}
                </PopOutCard>
            </div>
        </>
    );
};
