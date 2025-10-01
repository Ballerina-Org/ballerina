namespace Ballerina.Seeds.Test.SampleData

module Records =
  let nested =
    """{
  "PublicInfo": {
    "discriminator": "record",
    "value": [
      [
        {
          "discriminator": "lookup",
          "value": "First Name"
        },
        {
          "discriminator": "string"
        }
      ],
      [
        {
          "discriminator": "lookup",
          "value": "Nickname"
        },
        {
          "discriminator": "string"
        }
      ]
    ]
  },
  "PrivateInfo": {
    "discriminator": "record",
    "value": [
      [
        {
          "discriminator": "lookup",
          "value": "Age"
        },
        {
          "discriminator": "int32"
        }
      ],
      [
        {
          "discriminator": "lookup",
          "value": "Email"
        },
        {
          "discriminator": "string"
        }
      ]
    ]
  },
  "Person": {
    "discriminator": "record",
    "value": [
      [
        {
          "discriminator": "lookup",
          "value": "Public"
        },
        {
          "discriminator": "lookup",
          "value": "PublicInfo"
        }
      ],
      [
        {
          "discriminator": "lookup",
          "value": "Private"
        },
        {
          "discriminator": "lookup",
          "value": "PrivateInfo"
        }
      ]
    ]
  }
}"""

  let flatten =
    """{
  "CommonDetails": {
    "discriminator": "record",
    "value": [
      [
        {
          "discriminator": "lookup",
          "value": "Email"
        },
        {
          "discriminator": "string"
        }
      ],
      [
        {
          "discriminator": "lookup",
          "value": "Name"
        },
        {
          "discriminator": "string"
        }
      ]
    ]
  },
  "Person": {
    "discriminator": "flatten",
    "value": [
      {
        "discriminator": "record",
        "value": [
          [
            {
              "discriminator": "lookup",
              "value": "Age"
            },
            {
              "discriminator": "int32"
            }
          ]
        ]
      },
      {
        "discriminator": "lookup",
        "value":"CommonDetails"
      }
    ]
  }
}
"""
