import {useParams} from "react-router-dom";
import {useEffect, useState} from "react";
import Product from "../models/Product.ts";
import "../styles/EditProductPage.css"
import authClient from "../api/AuthClient.ts";
import StoreQuantityPair from "../models/StoreQuantityPair.ts";
import Store from "../models/Store.ts";
import Track from "../models/Track.ts";

export const EditProductPage = () => {
    const params = useParams();
    const [url, _] = useState(`/Product/GetProductById/${params.id}`);
    const [urlAvail, __] = useState(`/Product/GetAvailableStoresById/${params.id}`);
    const [product, setProduct] = useState<Product>();
    const [availability, setAvailability] = useState<Array<StoreQuantityPair>>();
    const [loadingP, setLoadingP] = useState(true);
    const [loadingA, setLoadingA] = useState(true);
    
    useEffect(() => {
        const fetchProduct = async () => {
            return await authClient.get(url);
        }
    
        const fetchQuantity = async () => {
            return await authClient.get(urlAvail);
        }

        fetchProduct().then(res => {
            setProduct(new Product(res.data.data));
            setLoadingP(false);
        })
            .catch((error) => console.log(error));

        fetchQuantity().then(res => {
            const l = new Array<StoreQuantityPair>();
            res.data.data.forEach(r => {
                l.push(new StoreQuantityPair(new Store(r.store), r.quantity));
            })
            setAvailability(l);
            setLoadingA(false);
        })
            .catch((error) => console.log(error));
        
    }, [url, urlAvail])

    const noData = () => {
        setTimeout(() => {
            return <h1>No Data</h1>
        }, 1000)
    }
    
    const handleCancel = () => {
        window.history.back();
    }
    
    const handleDelete = async () => {
        try {
            const res = await authClient.delete(`/Product/DeleteById/${params.id}`);
            console.log(res.data.data);
            window.history.back();
        }
        catch(e){
            console.error(e);
        }
    }
    
    const handleUpdate = async () => {
       try{
           const res = await authClient.put(`/Product/UpdateProduct`, {
               product,
               storeQuantities: availability
           });
           console.log(res.data.data);
           window.history.back();
       }
       catch(e){
           console.error(e);
       }
    }
    
    const handleRemoveSong = async () => {
        //TODO
    }
    
    const renderProduct = (product: Product) => {
        return (
            <div className="mainEditCard" style={{
                background: "url("+`${product.imageUrl}`+") center",
            }}>
                <div className="upperEditCard">
                    <div className="imageDiv">
                        <div>
                            <img src={product.imageUrl}
                                 width={200}
                                 height={200}
                                 alt={product.barcode + " cover"}/>
                        </div>
                    </div>
                    <div className="mainInforation">
                        <div className="infoField">
                            <p>Barcode: </p>
                            <p>{product.barcode}</p>
                        </div>
                        <div className="infoField">
                            <p>Catalog Number: </p>
                            <input type="text" value={product.catalogNumber} onChange={(e) => {
                                setProduct({...product, catalogNumber: e.target.value});
                            }}/>
                        </div>
                        <div className="infoField">
                            <p>Name: </p>
                            <input type="text" value={product.name} onChange={(e) => {
                                setProduct({...product, name: e.target.value});
                            }}/>
                        </div>
                        <div className="infoField">
                            <p>Artist: </p>
                            <input type="text" value={product.artist} onChange={(e) => {
                                setProduct({...product, artist: e.target.value});
                            }}/>
                        </div>
                        <div className="infoField">
                            <p>Price: </p>
                            <input type="text" value={product.price ?? ""} onChange={(e) => {
                                setProduct({...product, price: e.target.value});
                            }}/>
                        </div>
                        <div className="infoField">
                            <p>Type:</p>
                            <select onChange={(e) => {
                                setProduct({...product, type: e.target.selectedIndex})
                            }}>
                                {
                                    [0, 1, 2].map((item) => {
                                        return <option
                                            selected={item === product.type}>{Product.evaluateType(item)}</option>
                                    })
                                }
                            </select>
                        </div>
                    </div>
                </div>
                <div className="lowerEditCard">
                    <div>
                        <p>Tracklist:</p>
                        <div className="tracks">
                            {product.tracklist.map((t: Track, i: number) => {
                                return (
                                    <p key={i}>{i + 1}. 
                                        <input className="track" 
                                               type="text" 
                                               value={t.title}
                                               onChange={(e) => {
                                                   const t = [...product.tracklist];
                                                   t[i].title = e.target.value;
                                                   setProduct({...product, tracklist: t});
                                               }}/>
                                        <input className="track"
                                               type="text"
                                               value={t.runtime}
                                               onChange={(e) => {
                                                   const t = [...product.tracklist];
                                                   t[i].runtime = e.target.value;
                                                   setProduct({...product, tracklist: t});
                                               }}/>
                                        <button className="buttonRemoveSong deleteEdit"
                                                onClick={handleRemoveSong}>
                                            X
                                        </button>
                                    </p>
                                )
                            })}
                        </div>
                    </div>
                    <div>
                        <p>Availabilty:</p>
                        <div className="tracks">
                            {availability.map((a: StoreQuantityPair, i: number) => {
                                return (
                                    <p key={i}>{i + 1}. {a.store.name}
                                        <input className="track" 
                                               type="text" 
                                               value={a.quantity} 
                                               onChange={(e) => {
                                                   const t = [...availability];
                                                   t[i].quantity = Number.parseInt(e.target.value, 10);
                                                   setAvailability(t);
                                               }}/>
                                    </p>
                                )
                            })}
                        </div>
                    </div>
                </div>
                <div className="buttonsEdit">
                    <button className="buttonEdit cancelEdit" onClick={handleCancel}>Cancel Update</button>
                    <button className="buttonEdit updateEdit" onClick={handleUpdate}>Update Product</button>
                    <button className="buttonEdit deleteEdit" onClick={handleDelete}>Delete Product</button>
                </div>
            </div>
        )
    }
    
    return (
        <>
            {
                (!loadingA && !loadingP && product) ?
                renderProduct(product):
                noData()
            }
        </>
    )


} 