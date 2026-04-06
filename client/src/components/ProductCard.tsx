import "../styles/ProductCard.css"

export const ProductCard = ({product}) => {
    return (
        <div className="productCard">             
            <img src={product.imageUrl}
                 width={200}
                 height={200} 
                 alt="Epic album cover"/>
            <p> {product.artist} - {product.name}</p>
        </div>
    )
}