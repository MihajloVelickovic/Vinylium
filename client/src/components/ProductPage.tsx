import {useParams} from "react-router-dom";
import {useEffect, useState} from "react";
import axios from "axios";
import Product from "../models/Product.ts";

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
    
    return <h1>{product ? product.name : "No data"}</h1>    
    
    
} 