package ballerinareader

import (
	"errors"
	"reflect"
	"testing"

	"github.com/stretchr/testify/require"
)

func TestAll(t *testing.T) {
	type testCase[I any, O any] struct {
		name    string
		readers []ReaderWithError[I, O]
		want    ReaderWithError[I, []O]
		errMsg  string
	}
	tests := []testCase[string, string]{
		{
			name: "happy path",
			readers: []ReaderWithError[string, string]{
				NewReaderWithError[string, string](func(s string) (string, error) { return s + "1", nil }),
				NewReaderWithError[string, string](func(s string) (string, error) { return s + "2", nil }),
				NewReaderWithError[string, string](func(s string) (string, error) { return s + "3", nil }),
			},
			want: NewReaderWithError(func(s string) ([]string, error) { return []string{s + "1", s + "2", s + "3"}, nil }),
		},
		{
			name: "single reader",
			readers: []ReaderWithError[string, string]{
				NewReaderWithError(func(s string) (string, error) { return s + "_only", nil }),
			},
			want: NewReaderWithError(func(s string) ([]string, error) { return []string{s + "_only"}, nil }),
		},
		{
			name:    "empty slice",
			readers: []ReaderWithError[string, string]{},
			want:    NewReaderWithError(func(s string) ([]string, error) { return []string{}, nil }),
		},
		{
			name:    "nil readers slice",
			readers: nil,
			want:    NewReaderWithError(func(s string) ([]string, error) { return []string{}, nil }),
		},
		{
			name: "one reader errors",
			readers: []ReaderWithError[string, string]{
				NewReaderWithError(func(s string) (string, error) { return s + "1", nil }),
				NewReaderWithError(func(s string) (string, error) { return "", errors.New("boom") }),
				NewReaderWithError(func(s string) (string, error) { return s + "3", nil }),
			},
			want:   NewReaderWithError(func(s string) ([]string, error) { return nil, errors.New("boom") }),
			errMsg: "boom",
		},
		{
			name: "empty input",
			readers: []ReaderWithError[string, string]{
				NewReaderWithError(func(s string) (string, error) { return s + "A", nil }),
				NewReaderWithError(func(s string) (string, error) { return s + "B", nil }),
			},
			want: NewReaderWithError(func(s string) ([]string, error) { return []string{s + "A", s + "B"}, nil }),
		},
	}

	prefix := "prefix"
	for _, tt := range tests {
		t.Run(tt.name, func(t *testing.T) {
			if tt.errMsg == "" {
				gotApplied, err := All(tt.readers).Apply(prefix)
				require.NoError(t, err)

				wantApplied, err := tt.want.Apply(prefix)
				require.NoError(t, err)

				if !reflect.DeepEqual(gotApplied, wantApplied) {
					t.Errorf("All() = %v, want %v", gotApplied, wantApplied)
				}
			} else {
				_, err := All(tt.readers).Apply(prefix)
				require.ErrorContains(t, err, tt.errMsg)

				_, err = tt.want.Apply(prefix)
				require.ErrorContains(t, err, tt.errMsg)
			}
		})
	}

}
