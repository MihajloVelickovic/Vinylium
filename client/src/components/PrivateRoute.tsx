import {useAuth} from "./AuthContext.tsx";
import {Navigate, useParams} from "react-router-dom";

export const PrivateRoute = ({children}) => {
    const params = useParams();
    const paramsUsername = params.username;
    const {isLoggedIn, username} = useAuth();
    return isLoggedIn() && (paramsUsername === username) ? children : <Navigate to="/"/>;
}