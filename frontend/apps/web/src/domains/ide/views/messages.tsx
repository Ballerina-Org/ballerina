/** @jsxImportSource @emotion/react */

export const Messages: React.FC<{
    clientErrors?: string[];
    bridgeErrors?: string[];
}> =

    ({
         clientErrors = [],
         bridgeErrors = [],
     }) =>

    {

        return (
            <>
                <div className="toast">
                    {clientErrors.map((msg, i) => (
                        <div className="alert alert-error">
                            <span>{msg}</span>
                        </div>
                    ))}

                </div>
                <div className="toast">
                    {bridgeErrors.map((msg, i) => (
                        <div className="alert alert-error">
                            <span>{msg}</span>
                        </div>
                    ))}
 
                </div>

            </>
        )
    }