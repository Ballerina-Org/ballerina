namespace Ballerina.Seeds.Test.SampleData

module Records =
  let nested =
    """{
  "PublicInfo": {
    "kind": "record",
    "record": [
      [
        {
          "kind": "lookup",
          "lookup": "First Name"
        },
        {
          "kind": "string"
        }
      ],
      [
        {
          "kind": "lookup",
          "lookup": "Nickname"
        },
        {
          "kind": "string"
        }
      ]
    ]
  },
  "PrivateInfo": {
    "kind": "record",
    "record": [
      [
        {
          "kind": "lookup",
          "lookup": "Age"
        },
        {
          "kind": "int32"
        }
      ],
      [
        {
          "kind": "lookup",
          "lookup": "Email"
        },
        {
          "kind": "string"
        }
      ]
    ]
  },
  "Person": {
    "kind": "record",
    "record": [
      [
        {
          "kind": "lookup",
          "lookup": "Public"
        },
        {
          "kind": "lookup",
          "lookup": "PublicInfo"
        }
      ],
      [
        {
          "kind": "lookup",
          "lookup": "Private"
        },
        {
          "kind": "lookup",
          "lookup": "PrivateInfo"
        }
      ]
    ]
  }
}"""

  let flatten =
    """{
  "CommonDetails": {
    "kind": "record",
    "record": [
      [
        {
          "kind": "lookup",
          "lookup": "Email"
        },
        {
          "kind": "string"
        }
      ],
      [
        {
          "kind": "lookup",
          "lookup": "Name"
        },
        {
          "kind": "string"
        }
      ]
    ]
  },
  "Person": {
    "kind": "flatten",
    "flatten": [
      {
        "kind": "record",
        "record": [
          [
            {
              "kind": "lookup",
              "lookup": "Age"
            },
            {
              "kind": "int32"
            }
          ]
        ]
      },
      {
        "kind": "lookup",
        "lookup":"CommonDetails"
      }
    ]
  }
}
"""
