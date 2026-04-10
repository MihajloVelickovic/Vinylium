import axios from "axios";
import {useState} from "react";
import Product from "../models/Product";
import {AlbumCard} from "./AlbumCard";
import "../styles/FetchAlbumsForm.css"

export const FetchAlbumsForm = () => {

    const [code, setCode] = useState("");
    const [results, setResults] = useState(new Array<Product>());
    const [error, setError] = useState("");
    // flips value on every successful fetch 
    // is then passed to AlbumCard components
    // useEffect in there tracks the renderer variable
    // because it's changing on every fetch, useEffect gets called every time
    // thereby setting the focus on the input field needed
    const [rerender, setRerender] = useState(false);
    
    const handleSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError("");
        let result;
        try {
            result = await axios.post("http://localhost:1738/api/Product/FetchProducts", {
                    code
                }
            );
        } catch (e:any) {
            if(e.response && e.response.data)
                setError(e.response.data);
            console.log("Exception: " + e);
            setResults([])
            return;
        }

        const resultProducts = new Array<Product>();

        result.data.data.forEach((p: never) => resultProducts.push(new Product(p)))

        setResults(resultProducts);
        setRerender(!rerender);
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
                    <div>
                        <h2>Potential Best Match</h2>
                        <div className="albums best">
                            <AlbumCard product={results.at(0)} best={true} rerender={rerender}/>
                        </div>
                    </div>
                    :
                    <h2 className="fetchError">{error}</h2>
                }
            </div>
            {results.length > 1 &&
                <div className="fetchedAlbums">
                    <h2>Other matches</h2>
                    <div className="albums">
                        {results.slice(1).map((p: Product) => <AlbumCard product={p} best={false} rerender={rerender}/>)}
                    </div>
                </div>
            }
        </div>
    );

}