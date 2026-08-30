import {useParams} from "react-router-dom";
import {useEffect, useState} from "react";
import Product from "../models/Product.ts";
import "../styles/ProductPage.css"
import client from "../api/Client.ts";
import Track from "../models/Track.ts";
import Store from "../models/Store.ts";
import StoreQuantityPair from "../models/StoreQuantityPair.ts";
import {useCart} from "./CartContext.tsx";

export const ProductPage = () => {

    const params = useParams();
    const [url, _] = useState(`/Product/GetProductById/${params.id}`);
    const [product, setProduct] = useState<Product>();
    const [storeQuantities, setStoreQuantities] = useState<Array<StoreQuantityPair>>([]);
    const [selectedStoreId, setSelectedStoreId] = useState("");
    const {addItem} = useCart();

    useEffect(() => {
        const fetchData = async () => {
            return await client.get(url);
        }

        fetchData().then(res => {
            setProduct(new Product(res.data.data));
        })
            .catch((error) => console.log(error));

    }, [url])
    
    useEffect(() => {
        client.get(`/Product/GetAvailableStoresById/${params.id}`)
            .then(res => {
                const pairs = res.data.data.map((s: any) => new StoreQuantityPair(new Store(s.store), s.quantity));
                setStoreQuantities(pairs);
                const firstAvailable = pairs.find((p: StoreQuantityPair) => p.quantity > 0);
                setSelectedStoreId(firstAvailable?.store.id ?? "");
            })
            .catch((error) => console.log(error));
    }, [params.id])

    const selectedQuantity = storeQuantities.find(p => p.store.id === selectedStoreId)?.quantity ?? 0;

    const renderProduct = (product: Product) => {
        return (
            <div className="productPage">
                <div className="productCombined">

                    <div className="background-style-prod-page" style={{
                        background: "url(" + `${product.imageUrl}` + ")",
                        backgroundSize: "cover",
                        backgroundPosition: "center",
                        backgroundRepeat: "no-repeat",
                        position: "fixed"
                    }}>
                    </div>

                    <div className="productPageColumn">
                        <div className="mainInfo">
                            <div className="mainLeftSide">
                                <img src={product.imageUrl}
                                     width={400}
                                     height={400}
                                     alt={product.barcode + " cover"}/>

                                <div>
                                    <div className="productDetail">
                                        <p>Release Date</p>
                                        <p>{product.releaseDate}</p>
                                    </div>

                                    <div className="productDetail">
                                        <p>Format</p>
                                        <p id="type">{Product.evaluateType(product.type)}</p>
                                    </div>
                                    

                                    <div className="productDetail">
                                        <p>Runtime</p>
                                        <p>{product.runtime}</p>
                                    </div>
                                </div>
                            </div>
                            <div className="mainRightSide">
                                <div>
                                    <div className="title">
                                        <div>
                                            <h1>{product.artist}</h1>
                                        </div>
                                        <div>
                                            <h1>-</h1>
                                        </div>
                                        <div>
                                            <h1>{product.name}</h1>
                                        </div>
                                    </div>
                                </div>

                                <div className="priceQuantityInfo">
                                    <p className="price">{product.price} RSD</p>
                                </div>
                                <div className="storeSelect">
                                    <select value={selectedStoreId}
                                            onChange={e => setSelectedStoreId(e.target.value)}>
                                        <option value="" disabled>Select a store</option>
                                        {storeQuantities.map(p => (
                                            <option key={p.store.id} value={p.store.id} disabled={p.quantity === 0}>
                                                {p.store.name} ({p.quantity > 0 ? `${p.quantity} in stock` : "out of stock"})
                                            </option>
                                        ))}
                                    </select>
                                </div>
                                <div>
                                    <button className="add-to-cart" type="button"
                                            disabled={!product.inStock || !selectedStoreId || selectedQuantity === 0}
                                            onClick={() => addItem(product.barcode, selectedStoreId, 1)}>{
                                        product.inStock ?
                                            "Add to cart" :
                                            "Out of Stock"}
                                    </button>
                                </div>
                                <div className="tracklist">
                                    <div>
                                        <p>Tracklist: </p>
                                    </div>
                                    <div>
                                        {product.tracklist.map((t: Track, i: number) => {
                                            return (
                                                <div className="tracklistDetail">
                                                    <p>{i + 1}.</p><p>{t.title}</p><p>{t.runtime}</p>
                                                </div>
                                            )
                                        })}
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        )
    }

    const noData = () => {
        setTimeout(() => {
            return <h1>No Data</h1>
        }, 1000)
    }

    return (
        <>
            {
                product ?
                    renderProduct(product)
                    : noData()
            }
        </>
    )


} 