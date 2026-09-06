import "../styles/AlbumCard.css";
/* the card deliberately borrows EditProductPage's layout so the add and edit
 * screens read as the same thing at different stages */
import "../styles/EditProductPage.css";
import PopOutCard from "./PopOutCard.tsx";
import {useEffect, useRef, useState} from "react";
import Product from "../models/Product.ts";
import authClient from "../api/AuthClient";
import Store from "../models/Store.ts";
import StoreQuantityPair from "../models/StoreQuantityPair.ts";
import {useProductDraft} from "../hooks/useProductDraft.ts";
import Track from "../models/Track.ts";
import {apiError} from "../helpers/apiError.ts";
import {useToast} from "./ToastContext.tsx";

export const AlbumCard = ({product, best}: { product: Product, best: boolean }) => {
    const [isOpen, setIsOpen] = useState(false);
    const priceRef = useRef<HTMLInputElement>(null);

    const [priceHint, setPriceHint] = useState<string | null>("Set a price before adding");
    const [status, setStatus] = useState<{ kind: "ok" | "error", message: string } | null>(null);
    const [saving, setSaving] = useState(false);
    const [storeQuantities, setStoreQuantities] = useState<Array<StoreQuantityPair>>([]);
    const [storesFailed, setStoresFailed] = useState(false);
    const [loading, setLoading] = useState(true);

    const {draft, setField, toPayload, priceError} = useProductDraft(product);
    const {notify} = useToast();

    useEffect(() => {
        const getStores = async () => {
            const pairs = new Array<StoreQuantityPair>();

            try {
                const r = await authClient.get("/Store/GetAllStores");

                r.data.data.forEach((store: any) => {
                    const item = new StoreQuantityPair(new Store(store), 0);
                    pairs.push(item);
                });
            } catch (e) {
                console.error(e);
                setStoresFailed(true);
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

    const setQuantity = (storeId: string, quantity: string) => {
        setStoreQuantities(prev =>
            prev.map(p => {
                return p.store.id === storeId ?
                       new StoreQuantityPair(p.store, Math.max(0, parseInt(quantity) || 0)) :
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
        setStatus(null);

        if (storeQuantities.length === 0) {
            setStatus({
                kind: "error",
                message: storesFailed ?
                         "Could not load stores — refresh and try again" :
                         "Add a store before adding products to the catalogue"
            });
            return;
        }

        const invalid = priceError();
        if (invalid) {
            setPriceHint(invalid);
            priceRef.current?.focus();
            return;
        }

        setSaving(true);
        try {
            await authClient.post("/Product/AddProduct", {
                product: toPayload(),
                storeQuantities
            });
            setStatus({kind: "ok", message: `Added ${draft.name} to the catalogue`});
            notify(`Added "${draft.name}" to the catalogue`);
        } catch (e: any) {
            const message = apiError(e, "Could not add the product");
            if (message.toLowerCase().includes("price"))
                setPriceHint(message);
            else
                setStatus({kind: "error", message});
            notify(message, "error");
        } finally {
            setSaving(false);
        }
    };

    return (
        <>
            <div
                className={"mainEditCard" + (best ? " bestCard" : "")}
                style={{background: `url(${draft.imageUrl}) center`}}
                onKeyUp={(e) => {
                    if (e.key === "Escape" && isOpen)
                        setIsOpen(false);
                }}>

                <div className="upperEditCard">
                    <div className="imageDiv">
                        <div>
                            <img
                                src={draft.imageUrl}
                                width={200}
                                height={200}
                                alt={draft.barcode + " cover"}
                                style={{pointerEvents: "none"}}/>
                        </div>
                    </div>

                    <div className="mainInforation">
                        <div className="infoField">
                            <p>Barcode: </p>
                            <input type="text" spellCheck={false}
                                   value={draft.barcode}
                                   onChange={e => setField("barcode", e.target.value)}/>
                        </div>

                        <div className="infoField">
                            <p>Catalog Number: </p>
                            <input type="text" spellCheck={false}
                                   value={draft.catalogNumber}
                                   onChange={e => setField("catalogNumber", e.target.value)}/>
                        </div>

                        <div className="infoField">
                            <p>Name: </p>
                            <input type="text" spellCheck={false}
                                   value={draft.name}
                                   onChange={e => setField("name", e.target.value)}/>
                        </div>

                        <div className="infoField">
                            <p>Artist: </p>
                            <input type="text" spellCheck={false}
                                   value={draft.artist}
                                   onChange={e => setField("artist", e.target.value)}/>
                        </div>

                        <div className="infoField">
                            <p>Release Date: </p>
                            <input type="text" spellCheck={false}
                                   value={draft.releaseDate}
                                   onChange={e => setField("releaseDate", e.target.value)}/>
                        </div>

                        {/* priceRow anchors the hint tooltip to this row rather
                            than to the whole card */}
                        <div className="infoField priceRow">
                            <p>Price: </p>
                            {/* ?? "" keeps the input controlled from the first
                                render, otherwise React swaps it from
                                uncontrolled and drops the first character */}
                            <input type="text" inputMode="decimal" placeholder="0.00"
                                   ref={priceRef}
                                   aria-invalid={priceHint !== null}
                                   value={draft.price ?? ""}
                                   onChange={e => {
                                       setField("price", e.target.value);
                                       setPriceHint(null);
                                   }}/>
                            {priceHint && (
                                <div
                                    className="priceHint"
                                    role="tooltip"
                                    onClick={() => setPriceHint(null)}>
                                    {priceHint}
                                </div>
                            )}
                        </div>

                        <div className="infoField">
                            <p>Type:</p>
                            <select
                                value={draft.type}
                                onChange={e => setField("type", Number(e.target.value))}>
                                {[0, 1, 2].map(item => (
                                    <option key={item} value={item}>
                                        {Product.evaluateType(item)}
                                    </option>
                                ))}
                            </select>
                        </div>
                    </div>
                </div>

                <div className="lowerEditCard">
                    <div>
                        <p>Tracklist: {draft.runtime}</p>
                        <div className="tracks">
                            {draft.tracklist.map((track: Track, i: number) => (
                                <p key={i}>{i + 1}.
                                    <input className="track" type="text" spellCheck={false}
                                           value={track.title}
                                           onChange={e => updateTrack(i, "title", e.target.value)}/>

                                    <input className="track" type="text" spellCheck={false}
                                           value={track.runtime}
                                           onChange={e => updateTrack(i, "runtime", e.target.value)}/>

                                    <button className="buttonRemoveSong deleteEdit"
                                            type="button"
                                            onClick={() => handleRemoveSong(i)}>
                                        X
                                    </button>
                                </p>
                            ))}
                        </div>
                    </div>

                    <div>
                        <p>Availability:</p>
                        <div className="tracks">
                            {!loading && storeQuantities.length === 0 &&
                                <p className="noStores">
                                    {storesFailed ?
                                     "Could not load stores — refresh and try again" :
                                     "No stores yet — create one first"}
                                </p>}

                            {!loading &&
                                storeQuantities.map((s: StoreQuantityPair) => (
                                    <p key={s.store.id}>
                                        {s.store.name}
                                        <input className="track" type="text" inputMode="numeric"
                                               value={s.quantity.toString()}
                                               onChange={e => setQuantity(s.store.id, e.target.value)}/>
                                    </p>
                                ))}
                        </div>
                    </div>
                </div>

                <div className="buttonsEdit">
                    <button className="buttonEdit updateEdit"
                            type="button"
                            disabled={saving || loading || storeQuantities.length === 0}
                            title={storeQuantities.length === 0 && !loading ?
                                   (storesFailed ?
                                    "Could not load stores" :
                                    "There are no stores to stock this product in") :
                                   undefined}
                            onClick={acceptProduct}>
                        {saving ? "Adding…" : "Add Product"}
                    </button>

                    <button className="buttonEdit cancelEdit"
                            type="button"
                            onClick={() => setIsOpen(true)}>
                        Details
                    </button>
                </div>

                {status && (
                    <p className={"formStatus " + (status.kind === "ok" ? "formStatusOk" : "formStatusError")}
                       role={status.kind === "ok" ? "status" : "alert"}>
                        {status.message}
                    </p>
                )}
            </div>

            {/* the tracklist moved inline above, so the pop-out is now just the
                full-size artwork */}
            <div className="pop-out">
                <PopOutCard
                    isOpen={isOpen}
                    onClose={() => setIsOpen(false)}
                    title={draft.name}
                    backgroundImage={draft.imageUrl}>
                    <img
                        src={draft.imageUrl}
                        style={{width: "100%"}}
                        alt={draft.barcode + " cover"}/>
                </PopOutCard>
            </div>
        </>
    );
};
