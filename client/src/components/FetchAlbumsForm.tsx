import axios from "axios";
import {useState} from "react";
import Product from "../models/Product";
import {AlbumCard} from "./AlbumCard";
import "../styles/FetchAlbumsForm.css"

export const FetchAlbumsForm = () => {

    const [code, setCode] = useState("");
    const [isBarcode, setIsBarcode] = useState(true);
    const [price, setPrice] = useState(0);
    const [results, setResults] = useState(new Array<Product>());

    const handleSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        setResults([]);
        let result;

        try {
            result = await axios.post("http://localhost:1738/api/Product/FetchProducts", {
                    code,
                    isBarcode,
                    price
                }
            );
            console.log(result.data);
        } catch (e) {
            console.log("Exception: " + e);
            return;
        }

        const resultProducts = new Array<Product>();

        result.data.data.forEach((p: never) => resultProducts.push(new Product(p)))

        setResults(resultProducts);

    }

    const printRes = (result: Product) => {
        return <AlbumCard product={result}/>
    }

    return (
        <div className="fetchForm">
            <form onSubmit={handleSubmit}>
                <div>
                    <input type="text"
                           value={code}
                           placeholder="Code"
                           onChange={(f) => setCode(f.target.value)}/>
                </div>
                <div>
                    <input type="text"
                           placeholder="Price"
                           onChange={(f) => setPrice(parseInt(f.target.value))}/>
                </div>
                <div>
                    <button className="isBarcodeButton" type="button" onClick={() => setIsBarcode(!isBarcode)}>
                        {isBarcode ? "Barcode" : "Catalog Number"}
                    </button>
                </div>
                <div>
                    <button className="fetchButton" type="submit">Fetch</button>
                </div>
            </form>

            <div className="fetchedAlbums">
                {
                    results.map(result => printRes(result))
                }
            </div>

        </div>
    );

}