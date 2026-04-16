import argparse
import json

def main() -> None:
    parser = argparse.ArgumentParser(description="Calculate shipping quote.")
    parser.add_argument("--actual-weight", type=float, required=True)
    parser.add_argument("--billing-weight", type=float, required=True)
    parser.add_argument("--price-per-kg", type=float, required=True)
    args = parser.parse_args()

    chargeable_weight = max(args.actual_weight, args.billing_weight)
    estimated_price = round(chargeable_weight * args.price_per_kg, 2)
    print(json.dumps({
        "actualWeight": args.actual_weight,
        "billingWeight": args.billing_weight,
        "chargeableWeight": chargeable_weight,
        "estimatedPrice": estimated_price
    }, ensure_ascii=False))

if __name__ == "__main__":
    main()