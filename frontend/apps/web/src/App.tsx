import { useState } from "react";
import "./App.css";
import {
  UncleTemplate,
  ParentTemplate2,
  Uncle,
  Parent,
  Person,
  Address,
} from "playground-core";
import { ParentLayout2 } from "./domains/parent/views/parentLayout2";
import { UncleLayout } from "./domains/uncle/views/uncleLayout";
import { FormsApp } from "./FormsApp";
import { parse } from "ballerina-core";
import { DataDrivenFieldsApp } from "./DataDrivenFieldsApp";
import { DispatcherFormsApp } from "./DispatcherFormsApp";
import { DispatcherFormsAppTables } from "./DispatcherFormsAppTables.";
import { TraversalDispatchTest } from "./TraversalDispatchTest";

function App(props: {
  app:
    | "regular"
    | "forms"
    | "data-driven-fields"
    | "dispatcher-forms"
    | "dispatcher-forms-tables"
    | "traversal-dispatch-test";
}) {
  if (props.app == "forms") return FormsApp({});
  if (props.app == "data-driven-fields") return DataDrivenFieldsApp({});
  if (props.app == "dispatcher-forms") return DispatcherFormsApp({});
  if (props.app == "dispatcher-forms-tables")
    return DispatcherFormsAppTables({});
  if (props.app == "traversal-dispatch-test") return TraversalDispatchTest({});

  const [uncle, setUncle] = useState(Uncle.Default());
  const [parent, setParent] = useState(Parent.Default());
  const uncleForeignMutations = Uncle.ForeignMutations({
    setState: setUncle,
    context: uncle,
  });

  type P = {
    name: string;
    surname: string;
    age: number;
    married: boolean;
    address: { city: string; street: string; number: number };
  };
  const p: any = {
    name: "Pippo",
    surname: "Schmidt",
    age: 35,
    married: true,
    address: { city: "Amsterdam", street: "Balatonmeerlaan", number: 21 },
  };
  const p_wrong: any = {
    name: "Pippo",
    surname: "Schmidt",
    age: 35,
    married: true,
    address: { city: "Amsterdam", street: "Balatonmeerlaan" },
  };

  return (
    <div className="App">
      <h1>Ballerina 🩰</h1>
      <div className="card">
        This one parses:{" "}
        {JSON.stringify(
          parse<P>({
            name: "string",
            surname: "string",
            age: "number",
            married: "boolean",
            address: {
              city: "string",
              street: "string",
              number: "number",
            },
          })(p),
        )}
        <br />
        This one does not:{" "}
        {JSON.stringify(
          parse<P>({
            name: "string",
            surname: "string",
            age: "number",
            married: "boolean",
            address: {
              city: "string",
              street: "string",
              number: "number",
            },
          })(p_wrong),
        )}
        <table>
          <tbody>
            <tr>
              <td>
                <UncleTemplate
                  context={uncle}
                  setState={setUncle}
                  foreignMutations={{}}
                  view={UncleLayout}
                />
              </td>
            </tr>
            <tr>
              <td>
                <ParentTemplate2
                  context={parent}
                  setState={setParent}
                  foreignMutations={{
                    setFlag: uncleForeignMutations.overrideFlag,
                  }}
                  view={ParentLayout2}
                />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
}

export default App;
