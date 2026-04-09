import axios from "axios";
import {useState} from "react";
import Product from "../models/Product";
import {AlbumCard} from "./AlbumCard";
import "../styles/FetchAlbumsForm.css"

export const FetchAlbumsForm = () => {

    const [code, setCode] = useState("");
    const [results, setResults] = useState(new Array<Product>());

    const handleSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        setResults([]);
        let result;

        try {
            result = await axios.post("http://localhost:1738/api/Product/FetchProducts", {
                    code
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

    return (
        <div className="fetchForm">
            <div className="formAndBest">
                <form onSubmit={handleSubmit} className="form">
                        <input type="text"
                               value={code}
                               placeholder="Code"
                               onChange={(f) => setCode(f.target.value)}/>
                        <button className="fetchButton" type="submit">Fetch</button>
                </form>
                {results.length > 0 ?
                    <div style={{padding:"0",border:"none",margin:"0"}}>
                        <h2>Potential Best Match</h2>
                        <div className="albums best">
                            <AlbumCard product={results.at(0)} best={true}/>
                        </div>
                    </div>
                    :
                    <></>
                    
                }
            </div>


            {results.length > 1 &&
                <div className="fetchedAlbums">
                    <h2>Other matches</h2>
                    <div className="albums">
                        {results.slice(1).map((p: Product) => <AlbumCard product={p}/>)}
                    </div>
                </div>
            }
        </div>
    );

}