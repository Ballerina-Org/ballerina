import React from "react";

export const NoSpescInfo = (): React.ReactElement =>
    (<div role="alert" className="alert alert-warning">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" className="h-6 w-6 shrink-0 stroke-current">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path>
        </svg>
        <span>You have no existing specifications. Start by creating a one by choosing the name of uploading files</span>
    </div>)