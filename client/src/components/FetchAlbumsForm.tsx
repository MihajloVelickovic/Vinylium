import {useState} from "react";
import Product from "../models/Product";
import {AlbumCard} from "./AlbumCard";
import "../styles/FetchAlbumsForm.css"
import authClient from "../api/AuthClient";
import {AlbumMatchCard} from "./AlbumMatchCard.tsx";

export const FetchAlbumsForm = () => {

    const [code, setCode] = useState("");
    const [results, setResults] = useState(new Array<Product>());
    const [error, setError] = useState("");
    // flips value on every successful fetch 
    // is then passed to AlbumCard components
    // useEffect in there tracks the renderer variable
    // because it's changing on every fetch, useEffect gets called every time
    // thereby setting the focus on the input field needed
    const [fetchId, setFetchId] = useState(0);
    const [selected, setSelected] = useState(0);
    const [browsing, setBrowsing] = useState(false);
    
    const pickMatch = (index: number) => {
        setSelected(index);
        setBrowsing(false);
    }
    
    const handleSubmit = async (e: React.SyntheticEvent<HTMLFormElement>) => {
        e.preventDefault();
        setError("");
        let result;
        try {
            result = await authClient.post("/Product/FetchProducts", {
                    code
                }
            );
            const resultProducts = new Array<Product>();
            result.data.data.forEach((p: never) => resultProducts.push(new Product(p)))
            console.log(resultProducts);
            setResults(resultProducts);
            setSelected(0);
            setBrowsing(false);
            setFetchId(id => id + 1);
        } catch (e:any) {
            console.log(e.response.data.message);
            setError(e.response.data.message);
            setResults([])
            setBrowsing(false)
            return;
        }
    }

    return (
        <>
            <div className="fetchForm">
                <div className={"formAndBest" + (browsing ? " browsing" : "")}>
                    <form onSubmit={handleSubmit} className="form">
                            <input type="text"
                                   value={code}
                                   placeholder="Code"
                                   onChange={(f) => setCode(f.target.value)}/>
                            <button className="fetchButton" type="submit">Fetch</button>
                        
                    </form>
                    {results.length > 0 ?
                        <div className={"mainMatch" + (browsing ? " hidden" : "")}>
                            <h2>{selected === 0 ? "Top match" : `Match ${selected + 1} of ${results.length}`}</h2>
                            <AlbumCard key={`${fetchId}-${selected}`} product={results[selected]} best={selected === 0}/>
                            {results.length > 1 && <button className="altButton" onClick={()=>setBrowsing(true)}>
                                Not this one? See {results.length - 1} other matches
                            </button>
                            }
                        </div>
                        :
                        <h2 className="fetchError">{error}</h2>
                    }
                </div>
                {browsing &&
                    <div className="crate">
                        <button className="backButton" onClick={()=> setBrowsing(false)}>←Back</button>
                        <h2>Other matches</h2>
                        <div className="crateGrid">
                            {results
                                .map((p: Product, i: number) => ({p, i}))
                                .filter(({i}) => i !== selected)
                                .map(({p, i}) =>
                                    <AlbumMatchCard key={`${fetchId}-${i}`} product={p} rank={i + 1} onPick={() => pickMatch(i)}/>)}
                        </div>
                    </div>
                }
            </div>
        </>
    );

}