"""Package the verified plugin binary without game dependencies."""

import argparse
import sys
from release_tools import package_release


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--tag", help="Optional GitHub release tag; must match the project version.")
    args = parser.parse_args()
    try:
        paths = package_release(args.tag)
    except (OSError, ValueError) as error:
        print("Packaging failed: " + str(error), file=sys.stderr)
        return 1
    for path in paths:
        print(path)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
