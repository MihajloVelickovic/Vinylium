import {useParams} from "react-router-dom";
import {useEffect, useState} from "react";
import Product from "../models/Product.ts";
import "../styles/EditProductPage.css"
import authClient from "../api/AuthClient.ts";

export const EditProductPage = () => {

    const params = useParams();
    const [url, _] = useState(`/Product/GetProductById/${params.id}`);
    const [product, setProduct] = useState<Product>();

    useEffect(() => {
        const fetchData = async () => {
            return await authClient.get(url);
        }

        fetchData().then(res => {
            setProduct(new Product(res.data.data));
        })
            .catch((error) => console.log(error));

    }, [url])

    const renderProduct = (product: Product) => {
        return (
            
            <div className="mainEditCard">
                <div className="upper">
                    <div className="image">
                        <img src={product.imageUrl}
                             width={200}
                             height={200}
                             alt={product.barcode + " cover"}/>
                    </div>
                    <div className="mainInfo">
                        <div className="infoField">
                            <p>Name: </p>
                            <input type="text" value={product.name} />
                        </div>
                        <div className="infoField">
                            <p>Artist: </p>
                            <input type="text" value={product.artist} />
                        </div>
                        <div className="infoField">
                            <p>Barcode: </p>
                            <input type="text" value={product.barcode} />
                        </div>
                        <div className="infoField">
                            <p>Catalog Number: </p>
                            <input type="text" value={product.catalogNumber} />
                        </div>
                        <div className="infoField">
                            <p>Price: </p>
                            <input type="text" value={product.price ?? ""} />
                        </div>
                        <div className="infoField">
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
                        </div><div className="infoField">
                        <p>Name: </p>
                        <input type="text" value={product.name} />
                    </div>
                        
                    </div>
                </div>
                <div className = "lower">
                    <div className="infoField">
                        <p>Tracklist: </p>
                        {product.tracklist.map((t: string, i: number) => {
                            return (
                                <>
                                <p>{i + 1}. <input className="track" type="text" value={t}/></p>
                                    <br/>
                                </>
                            )
                        })}
                    </div>
                    <div>
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