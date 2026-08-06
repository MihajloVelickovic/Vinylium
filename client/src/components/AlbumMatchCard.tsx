import "../styles/AlbumMatchCard.css"
import Product from "../models/Product.ts";

interface AlbumMatchCardProps {
    product: Product;
    rank: number;
    onPick: () => void;
}

export const AlbumMatchCard = ({product, rank, onPick}: AlbumMatchCardProps) => {
    return (
        <button type="button" className="matchCard" onClick={onPick}>
            <img className="matchArt" src={product.imageUrl} alt="" width={180} height={180}/>
            <span className="matchTitle">{rank}. {product.name}</span>
            <span className="matchArtist">{product.artist}</span>
        </button>
    );
}
