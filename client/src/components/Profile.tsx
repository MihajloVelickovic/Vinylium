import {useParams} from "react-router-dom";
import {useAuth} from "./AuthContext.tsx";
import "../styles/Profile.css"

export const Profile = () => {
    
    const {logout} = useAuth();
    
    //const params = useParams();
    
    return (
        <div className="profile">
            <button className="button-main" onClick={logout}>Logout</button>
            <button className="button-main">Edit</button>
        </div>
    )
}