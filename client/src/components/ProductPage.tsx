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
        
        fetchData().then(res => {setProduct(new Product(res.data.data));})
            .catch((error) => console.log(error));
        
    }, [url])

    const renderProduct = (product: Product) => {
        return (
            <div className="productPage">
                <div className="productCombined">
                    
                    <div className="background-style-prod-page" style={{
                        background: "url(" + `${product.imageUrl}` + ") center"
                    }}>
                    </div>
                    
                    <div className="productPageColumn left">                
                        <div>
                            
                            <img src={product.imageUrl} 
                                 width={400}
                                 height={400}
                                 alt={product.barcode + " cover"} />
                            
                            <div className="productDetail">
                                <p>Release Date:</p>
                                <p>{product.releaseDate}</p>
                            </div>
                            
                            <div className="productDetail">
                                <p>Runtime:</p>
                                <p>{product.runtime}</p>
                            </div>
                            
                            <div className="productDetail">
                                <p>Tracklist: </p>
                                {product.tracklist.map((t: string, i: number) => {
                                    return(
                                    <>
                                        <p>{i+1}. {t}</p>
                                        <br/>
                                    </>
                                    )
                                })}
                            </div>
                            
                        </div>
                        
                    </div>
                    
                    <div className="productPageColumn right">
                        <div className="title">
                            <h1>{product.artist}</h1>
                            <h1>-</h1>
                            <h1>{product.name}</h1>
                        </div>
                        <div className="priceType">
                            <p id="type">{product.evaluateType(product.type)}</p>
                            <p></p>
                            <p id="price">{product.price} RSD</p>
                        </div>
                        <button className="button-main">Add to cart</button>
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
        
    return(
        <>
            {
                product ?
                renderProduct(product)
                : noData()
            }
        </>
    )
    
    
        
} 