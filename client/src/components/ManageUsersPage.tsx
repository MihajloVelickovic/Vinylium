import "../styles/ManageUsersPage.css"
import authClient from "../api/AuthClient.ts";
import {UserCard} from "./UserCard.tsx";
import {Filters} from "./Filters.tsx";
import {UserFilterFields} from "./UserFilterFields.tsx";
import {useUserFilters} from "../hooks/useUserFilters.ts";

export const ManageUsersPage = () => {

    const {users, filters, setFilters, change, setChange, searchRef, error} = useUserFilters(authClient);

    return (
        <>
            <Filters searchRef={searchRef} params={{filters, setFilters, change, setChange}}>
                <UserFilterFields params={{filters, setFilters, change, setChange}}/>
            </Filters>
            <div className="users">
                {
                    error ?
                        <h3>{error}</h3> :
                        users.map(user => {
                            return <UserCard user={user}/>
                        })
                }
            </div>
        </>
    )

}
