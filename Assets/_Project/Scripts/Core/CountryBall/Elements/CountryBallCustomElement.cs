using UnityEngine;

public class CountryBallCustomElement : CountryBallElement
{
    [SerializeField] private CustomizationHatType customizationType;
    
    public CustomizationHatType CustomizationType => customizationType;
}