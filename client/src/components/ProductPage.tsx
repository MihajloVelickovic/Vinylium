import {useParams} from "react-router-dom";
import {useEffect, useState} from "react";
import axios from "axios";
import Product from "../models/Product.ts";
import "../styles/ProductPage.css"

export const ProductPage = () => {

    const params = useParams();
    const [url, _] = useState(`http://localhost:1738/api/Product/GetProductById/${params.id}`);
    const [product, setProduct] = useState<Product>();

    useEffect(() => {
        const fetchData = async () => {
            return await axios.get(url);
        }

        fetchData().then(res => {
            setProduct(new Product(res.data.data));
        })
            .catch((error) => console.log(error));

    }, [url])

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
                                    <div className="title">
                                        <p>{product.price} RSD</p>
                                        <></>
                                    </div>
                                </div>
                                <div>
                                    <button className="button-main" type="button">Add to cart</button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div className="productPageColumn">
                        <div className="productDetail">
                            <p>Tracklist: </p>
                            {product.tracklist.map((t: string, i: number) => {
                                return (
                                    <>
                                        <p>{i + 1}. {t}</p>
                                        <br/>
                                    </>
                                )
                            })}
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